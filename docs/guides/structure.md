# Advanced structure

[TypeSafe's structure contract](https://docs.typesafe.ai/primitives/advanced) permits text, objects, arrays, and null in instructions and descriptions. The SDK represents these with `JsonContent`. Nested properties can contain strings, finite numbers, booleans, arrays, objects, and null. JSON stays structured on the wire; it is not converted into a quoted JSON string.

| Content | .NET API |
|---|---|
| State | `SystemOneAsync(JsonContent, ...)` |
| Instructions | All four question types accept `JsonContent` |
| Choice descriptions | Dictionary values or `ChoiceQuestion.FromCriteria` |
| Score levels | Ordered `JsonContent` values or `ScoreQuestion.FromCriteria` |
| Noul definitions | `NoulCriteria(False: ..., True: ...)` |
| Score descriptions returned by the API | `ScoreAnswer.Legend`, containing `JsonElement` values |

## Structured questions and rubrics

```csharp
using TypeSafeAI;

var instructions = JsonContent.Parse("""
    {
      "question": "Does the sender match the organization?",
      "compare": ["sender.name", "sender.email"],
      "rules": {"allow_aliases": true, "missing_value": null}
    }
    """);
var questions = new Dictionary<string, Question>
{
    ["sender_matches"] = new NoulQuestion(instructions, new NoulCriteria(
        False: JsonContent.Parse("""{"meaning":"unrelated sender","examples":["unrecognized domain"]}"""),
        True: JsonContent.Parse("""{"meaning":"matching organization","examples":["verified alias"]}"""))),
    ["department"] = ChoiceQuestion.FromCriteria(JsonContent.Parse("""
        {
          "billing": {"includes":["duplicate charges","refunds"],"excludes":["delivery tracking"]},
          "delivery": ["late shipment", {"status":"label created"}],
          "other": null
        }
        """), "Which team should handle the message?"),
    ["severity"] = ScoreQuestion.FromCriteria(JsonContent.Parse("""
        [
          {"summary":"cosmetic","examples":["misaligned icon"]},
          {"summary":"workaround available","examples":["another browser works"]},
          {"summary":"blocked","examples":["all sign-ins fail"]}
        ]
        """), JsonContent.Parse("""["Judge impact",{"focus":"availability"}]""")),
};

using var client = new TypeSafeClient(apiKey);
var response = await client.SystemOneAsync(JsonContent.Parse("""
    {"sender":{"name":"Example Ltd","email":"support@example.test"},"message":"I was charged twice."}
    """), questions);
var description = response.Get<ScoreAnswer>("severity").Legend["1"];
Console.WriteLine(description.GetProperty("summary").GetString());
```

The property names inside your instructions and descriptions are yours. They are not a schema the SDK imposes on the model. Images can appear as references at any nested value or inside a string; see [Images and Box](images.md).

Passing `null` for nullable instructions omits the property. Passing `JsonContent.Null` explicitly writes JSON null. A null Choice dictionary value writes an undescribed label. Score arrays may contain `JsonContent.Null`; Noul definitions can be omitted independently or explicitly set to JSON null. The provider validates which combinations make a meaningful question.

For an existing DOM, use `JsonContent.FromNode(jsonNode)` or the implicit `JsonElement` conversion. For application-owned records, use `JsonContent.Create(record, MyJsonContext.Default.MyRecord)` with source-generated metadata. These avoid reflection and support NativeAOT.

## Walk a taxonomy

`ChoiceQuestion.FromCriteria` accepts an existing object directly, with each key becoming a label and each subtree remaining its structured description:

```csharp
var taxonomy = JsonContent.Parse("""
    {
      "outdoors":{"cycling":{"bottles":null}},
      "kitchen":{"drinkware":{"bottles":null,"mugs":null}}
    }
    """);
var question = ChoiceQuestion.FromCriteria(taxonomy, "Which branch fits the product?");
var answer = await client.SystemOneAsync("A bottle with a bicycle-cage mount.",
    new Dictionary<string, Question> { ["branch"] = question });
var branch = answer.Get<ChoiceAnswer>("branch");
var subtree = taxonomy.Value.GetProperty(branch.Choice);
var nextQuestion = ChoiceQuestion.FromCriteria(subtree, "Which subcategory fits?");
```

Call again with the next question to descend a level. Use the full `Probabilities` dictionary to explore several branches when appropriate. Stop at a leaf and keep confidence thresholds in application code. `FromCriteria` requires a nonempty object for Choice and a nonempty ordered array for Score.

## Limits and nesting

The [HTTP contract](https://docs.typesafe.ai/api#request-rules) permits up to 64 nested object/array levels in each state, instructions, and criteria field. The SDK leaves room for the surrounding request and response envelopes so valid content at that limit survives upload, structured Score legends, and replay.

JSON parsing rejects malformed JSON and comments. Non-finite numbers are invalid; exact identifiers or quantities beyond ordinary numeric precision should be strings. The provider enforces body/context limits and currently caps Choice at 255 labels and Score at 10 levels. Keep each level independently descriptive; adjacent level numbers do not provide semantic context to the model.

For custom response projections containing deeply nested `JsonElement` legends, configure your source-generated context with `MaxDepth = 80` as well, since your projection adds its own object envelope. Caller-supplied serializer settings remain under your application's control.

## Compose answers in code

Questions in one request share state and run independently. They cannot consume another question's answer from that same request. For a cascade or taxonomy traversal, inspect the first response and issue a second request with the additional state or narrower questions.

- Speculative fan-out: add all potentially useful questions to one `QuestionSet`, then use only relevant answers.
- Confidence routing: use Choice/Score `Confidence` and full `Probabilities`. A Noul returns its probability directly; it has no separate server confidence.
- Composite scoring: divide each Score by its highest level index (`criteria.Count - 1`) before combining scales of different lengths. Weights belong in application code.
- Intent routing: use typed enum handles, `TypeSafeIntentRouter<TEnum>`, or the [MEAI routing adapter](meai.md).
- Extraction, re-ranking, verification, and memory: create named questions over structured state, then resolve spans, dates, ranking, storage, or escalation in your application. These are compositions of the same HTTP endpoint rather than additional provider endpoints.

See the [documentation coverage audit](../reference-coverage.md) for every upstream page and its .NET mapping.
