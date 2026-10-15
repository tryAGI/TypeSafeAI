# Documentation coverage audit

Audited on 2026-10-15 against all **128 pages** in the provider's [documentation index](https://docs.typesafe.ai/llms.txt), including concepts, primitives, patterns, cookbooks, HTTP reference, Python/JavaScript SDK references, and changelogs. The live OpenAPI has two operations: `GET /v1/models` and `POST /v1/systemone`; both are implemented. The linked source pages remain authoritative for current model limits and behavior.

## Evidence and boundaries

| Capability | .NET surface | Regression evidence |
|---|---|---|
| All documented question/answer types | Noul, Choice, Score, Box; QuestionSet and grouped answers | Tests.Transport.cs, Tests.Images.cs |
| Text/object/array state and arbitrary nested JSON values | JsonContent.Parse/FromNode/Create with source-generated metadata | Tests.Structure.cs, Examples/StructuredState.cs |
| Text/object/array/null instructions and all criteria positions | JsonContent, NoulCriteria, ChoiceQuestion.FromCriteria, ScoreQuestion.FromCriteria | AdvancedStructurePreservesEveryEntryShapeAndStructuredScoreLegends |
| Full field nesting limit, with room for request/response envelopes | 64 content levels; envelope allowance | SixtyFourLevelsRemainValidInsideRequestAndResponseEnvelopes |
| Structured Score legends and replay | ScoreAnswer.Legend, RawJson, FromHttpResponse | Tests.Structure.cs |
| Images, reference placement, multipart/base64, retries | ImageAsset, RequestOptions.Images | Tests.Images.cs |
| Model aliases, discovery, response version | model argument, Models.ListAsync, SystemOneResponse.Model | Examples/ListModels.cs; live example requires credentials |
| Experimental long context | RequestOptions.EnableLongContext or default headers | CompressionAndLongContextSurviveRetries |
| Request compression | RequestCompression.Gzip/None; JSON and multipart; 1 KiB threshold | Tests.RequestFeatures.cs |
| Nullable/missing usage counts | Usage.InputTokens/OutputTokens; preserved raw JSON | Tests.Usage.cs |
| Status errors, retry-after/backoff, cancellation | Exceptions, RetryPolicy, CancellationToken | Tests.Transport.cs, Tests.RequestFeatures.cs, Tests.Contract.cs |
| Raw/future questions and responses | RawQuestion, RawAnswer, ExtraBody, RawJson | GeneratedTransportPreservesKnownAndFutureUnionValues; Tests.Contract.cs |
| Named/custom response projections | QuestionSet handles and caller JsonTypeInfo | Tests.Projection.cs, Tests.Images.cs, NativeAotSmoke |
| Confidence routing, normalized composition, taxonomy traversal | Existing answer properties plus application code | CriteriaFactoriesSupportTaxonomyWalkAndOrderedRubrics; Tests.Meai.cs |
| Bounded fan-out, multiple states, DI, transport injection | QuestionSet, SystemOneManyAsync, ITypeSafeClient, HttpClient | Examples/BatchStates.cs, Tests.Meai.cs |
| Logging and telemetry | ILoggerFactory, ActivitySource, Meter | Client implementation; no payload logging |
| Trimming/NativeAOT | Source-generated serialization; no reflection-based content factories | NativeAotSmoke; CI trimming checks |

Tests are local HTTP contract fixtures. They establish serialization, parsing, and composition behavior; they do not establish provider accuracy, live image acceptance, cost/latency claims, all cookbook datasets, or production acceptance. Credentialed examples skip when credentials are absent. Model limits, pricing, legal terms, and benchmarks are provider facts, not SDK features to hard-code.

## Runtime differences

- .NET uses documented gzip compression on request; it does not bundle Node/Python's native zstd runtime. Compression defaults to none and can be overridden per call.
- Image uploads use an explicit reference-to-asset map. Python inline objects and JavaScript tagged templates have the same wire capability through ImageAsset.Reference and RequestOptions.Images; their language syntax is not reproduced.
- Synchronous and asynchronous .NET APIs use tasks, CancellationToken, IDisposable, HttpClient and DI rather than Python event loops or JavaScript promises/fetch/browser guards.
- ILoggerFactory controls logs; environment log-level setup and raw body logging from the other SDKs are not copied. Request/response payloads remain out of SDK logs.
- Retry defaults and .NET option replacement differ from JS partial objects. A total retry budget is configurable and can be disabled explicitly.
- Score probability/legend keys retain the wire's string indices; enum helpers provide typed levels. Python's integer-key dictionaries are a language adaptation.
- Nullable token counts follow the SDK references, although the current OpenAPI still declares integers. A handwritten compatibility converter preserves the original JSON without editing generated code. The generated raw client follows its OpenAPI schema when used independently.
- Cookbook applications combine the SDK with caller-owned extraction, storage, ranking, UI, calendar math, or external models. The SDK supplies the TypeSafe calls and data needed for those workflows; it does not install or execute those applications or agent skills.

## Cookbook mappings

| Cookbook | Supported composition |
|---|---|
| [Function calling](https://docs.typesafe.ai/cookbooks/function_calling) | Choice/Noul, typed handles; application validates and dispatches closed-set arguments |
| [Skill suggestion](https://docs.typesafe.ai/cookbooks/skill_suggestion) | Score/Choice, distributions; application ranks candidates and performs the second pass |
| [Guardrails for LLMs](https://docs.typesafe.ai/cookbooks/llm_guardrails) | Mixed Noul/Score, MEAI guardrail policies and caller-selected thresholds |
| [Double-checking citations](https://docs.typesafe.ai/cookbooks/citation_check) | Structured state and Choice; application locates source spans |
| [Classifying RAG passages](https://docs.typesafe.ai/cookbooks/classifying_rag_passages) | Mixed named questions; application filters and assembles passages |
| [Simple memory system](https://docs.typesafe.ai/cookbooks/basic_memory) | Structured state, Noul and sequential requests; application persists memories |
| [Hierarchical classification](https://docs.typesafe.ai/cookbooks/hierarchical_classification) | Structured Choice criteria, full probabilities and sequential/parallel requests; application walks or beams the tree |
| [Classification using confidence](https://docs.typesafe.ai/cookbooks/classification_using_confidence) | Choice confidence; application chooses leaf versus parent category |
| [Twitch chat filter](https://docs.typesafe.ai/cookbooks/twitch_chat_filter) | Mixed questions, SystemOneManyAsync concurrency; application renders filtering decisions |
| [SDE cascade](https://docs.typesafe.ai/cookbooks/sde_cascade) | Structured instructions/Noul criteria; application calls extraction and escalation models |
| [Date extraction](https://docs.typesafe.ai/cookbooks/date_extraction_cookbook) | Choice/Noul closed sets; application performs calendar arithmetic |
| [Pre-parsed value extraction](https://docs.typesafe.ai/cookbooks/pre_parsed_value_extraction_cookbook) | Choice over candidate labels; application parses and normalizes verbatim spans |
| [Structure recovery](https://docs.typesafe.ai/cookbooks/autoformat) | Noul/Choice plus two sequential requests; application renders Markdown |
| [Re-ranking](https://docs.typesafe.ai/cookbooks/rerank_typesafe) | Score/Noul and bounded batches; application sorts candidate passages |
| [Line-by-line search](https://docs.typesafe.ai/cookbooks/semantic_find) | Structured line maps, Choice and Noul; application resolves chosen IDs |
| [Knowledge graph entity alignment](https://docs.typesafe.ai/cookbooks/entity_alignment) | Structured pairs, Score plus Noul; application thresholds and routes |
| [Self-consistency: choices](https://docs.typesafe.ai/cookbooks/consistency_choice_cookbook) | Choice distributions and confidence; application caches samples and defines abstention |
| [Self-consistency: nouls](https://docs.typesafe.ai/cookbooks/consistency_noul_cookbook) | Noul probabilities; application defines uncertainty bands |
| [Parallel questions](https://docs.typesafe.ai/cookbooks/parallel_questions) | QuestionSet for one state and many mixed questions; application compares batches |
| [Autoresearch feature discovery](https://docs.typesafe.ai/cookbooks/autoresearch_feature_discovery) | Mixed named questions and distributions; external model proposes features and application fits downstream models |

## Complete source inventory

Each indexed page is accounted for below. "Runtime reference" entries map to the capabilities and runtime differences above; "application pattern" entries use the cookbook mappings or ordinary composition over named answers. Documentation navigation, demos, agent installation, conceptual background, and legal material introduce no additional SDK endpoint.

| Upstream page | Coverage category |
|---|---|
| [Introduction](https://docs.typesafe.ai/introduction) | Core System One API, typed questions/answers and README |
| [Quick start](https://docs.typesafe.ai/introduction/quickstart) | Core System One API, typed questions/answers and README |
| [Jev with coding agents](https://docs.typesafe.ai/introduction/coding-agents) | Background/external application; no additional SDK operation |
| [Example use cases](https://docs.typesafe.ai/concepts/use-case-map) | Background/external application; no additional SDK operation |
| [System One](https://docs.typesafe.ai/concepts/system-one) | Core System One API, typed questions/answers and README |
| [State](https://docs.typesafe.ai/concepts/state) | Advanced structure guide and Tests.Structure.cs |
| [Primitives (Questions)](https://docs.typesafe.ai/primitives) | Core System One API, typed questions/answers and README |
| [Choice](https://docs.typesafe.ai/primitives/choice) | Core System One API, typed questions/answers and README |
| [Score](https://docs.typesafe.ai/primitives/score) | Core System One API, typed questions/answers and README |
| [Noul](https://docs.typesafe.ai/primitives/noul) | Core System One API, typed questions/answers and README |
| [Box](https://docs.typesafe.ai/primitives/box) | Images and Box guide and Tests.Images.cs |
| [Advanced: structure](https://docs.typesafe.ai/primitives/advanced) | Advanced structure guide and Tests.Structure.cs |
| [Images](https://docs.typesafe.ai/images) | Images and Box guide and Tests.Images.cs |
| [Confidence](https://docs.typesafe.ai/confidence) | Application composition, answer distributions, MEAI adapters |
| [How to build with TypeSafe](https://docs.typesafe.ai/concepts/how-to-build-with-system-one) | Application composition, answer distributions, MEAI adapters |
| [AI primer](https://docs.typesafe.ai/introduction/machine-learning-primer) | Background/external application; no additional SDK operation |
| [Patterns](https://docs.typesafe.ai/patterns) | Application composition, answer distributions, MEAI adapters |
| [Speculative fan-out](https://docs.typesafe.ai/patterns/fan-out) | Application composition, answer distributions, MEAI adapters |
| [Confidence-gated routing](https://docs.typesafe.ai/patterns/confidence-routing) | Application composition, answer distributions, MEAI adapters |
| [Composite scoring](https://docs.typesafe.ai/patterns/composite-scoring) | Application composition, answer distributions, MEAI adapters |
| [Intent routing](https://docs.typesafe.ai/patterns/intent-routing) | Application composition, answer distributions, MEAI adapters |
| [Cookbooks](https://docs.typesafe.ai/cookbooks) | Application composition, answer distributions, MEAI adapters |
| [Function calling](https://docs.typesafe.ai/cookbooks/function_calling) | Application pattern; see cookbook mapping |
| [Skill suggestion](https://docs.typesafe.ai/cookbooks/skill_suggestion) | Application pattern; see cookbook mapping |
| [Guardrails for LLMs](https://docs.typesafe.ai/cookbooks/llm_guardrails) | Application pattern; see cookbook mapping |
| [Double-checking citations](https://docs.typesafe.ai/cookbooks/citation_check) | Application pattern; see cookbook mapping |
| [Classifying RAG passages](https://docs.typesafe.ai/cookbooks/classifying_rag_passages) | Application pattern; see cookbook mapping |
| [Simple memory system](https://docs.typesafe.ai/cookbooks/basic_memory) | Application pattern; see cookbook mapping |
| [Hierarchical classification](https://docs.typesafe.ai/cookbooks/hierarchical_classification) | Application pattern; see cookbook mapping |
| [Classification using confidence](https://docs.typesafe.ai/cookbooks/classification_using_confidence) | Application pattern; see cookbook mapping |
| [Twitch chat filter](https://docs.typesafe.ai/cookbooks/twitch_chat_filter) | Application pattern; see cookbook mapping |
| [SDE cascade](https://docs.typesafe.ai/cookbooks/sde_cascade) | Application pattern; see cookbook mapping |
| [Date extraction](https://docs.typesafe.ai/cookbooks/date_extraction_cookbook) | Application pattern; see cookbook mapping |
| [Pre-parsed value extraction](https://docs.typesafe.ai/cookbooks/pre_parsed_value_extraction_cookbook) | Application pattern; see cookbook mapping |
| [Structure recovery](https://docs.typesafe.ai/cookbooks/autoformat) | Application pattern; see cookbook mapping |
| [Re-ranking](https://docs.typesafe.ai/cookbooks/rerank_typesafe) | Application pattern; see cookbook mapping |
| [Line-by-line search](https://docs.typesafe.ai/cookbooks/semantic_find) | Application pattern; see cookbook mapping |
| [Knowledge graph entity alignment](https://docs.typesafe.ai/cookbooks/entity_alignment) | Application pattern; see cookbook mapping |
| [Self-consistency: choices](https://docs.typesafe.ai/cookbooks/consistency_choice_cookbook) | Application pattern; see cookbook mapping |
| [Self-consistency: nouls](https://docs.typesafe.ai/cookbooks/consistency_noul_cookbook) | Application pattern; see cookbook mapping |
| [Parallel questions](https://docs.typesafe.ai/cookbooks/parallel_questions) | Application pattern; see cookbook mapping |
| [Autoresearch feature discovery](https://docs.typesafe.ai/cookbooks/autoresearch_feature_discovery) | Application pattern; see cookbook mapping |
| [Demos](https://docs.typesafe.ai/demos) | Background/external application; no additional SDK operation |
| [Smart home assistant demo](https://docs.typesafe.ai/demos/smart-home) | Background/external application; no additional SDK operation |
| [Models](https://docs.typesafe.ai/models) | Models.ListAsync, model selection, long-context header; provider owns limits |
| [API reference](https://docs.typesafe.ai/api) | Both HTTP operations, request/answer types, errors, compression |
| [Agent skill](https://docs.typesafe.ai/agent-skill) | Background/external application; no additional SDK operation |
| [Legal](https://docs.typesafe.ai/legal) | Background/external application; no additional SDK operation |
| [Jev 1.13 jaggedness](https://docs.typesafe.ai/model-jaggedness/jev-1.13) | Background/external application; no additional SDK operation |
| [Client SDKs](https://docs.typesafe.ai/sdk) | SDK installation; NuGet package and README |
| [TypeSafe Python SDK](https://docs.typesafe.ai/sdk/python) | Runtime reference; .NET equivalents and differences above |
| [Basic usage](https://docs.typesafe.ai/sdk/python/usage/basic) | Runtime reference; .NET equivalents and differences above |
| [Advanced usage](https://docs.typesafe.ai/sdk/python/usage/advanced) | Runtime reference; .NET equivalents and differences above |
| [Changelog](https://docs.typesafe.ai/sdk/python/changelog) | Runtime reference; .NET equivalents and differences above |
| [API reference](https://docs.typesafe.ai/sdk/python/api) | Runtime reference; .NET equivalents and differences above |
| [Asynchronous client](https://docs.typesafe.ai/sdk/python/api/clients/async) | Runtime reference; .NET equivalents and differences above |
| [Synchronous client](https://docs.typesafe.ai/sdk/python/api/clients/sync) | Runtime reference; .NET equivalents and differences above |
| [Questions](https://docs.typesafe.ai/sdk/python/api/types/questions) | Runtime reference; .NET equivalents and differences above |
| [Assets](https://docs.typesafe.ai/sdk/python/api/types/assets) | Runtime reference; .NET equivalents and differences above |
| [Answers and responses](https://docs.typesafe.ai/sdk/python/api/types/responses) | Runtime reference; .NET equivalents and differences above |
| [Retries](https://docs.typesafe.ai/sdk/python/api/retries) | Runtime reference; .NET equivalents and differences above |
| [Common types](https://docs.typesafe.ai/sdk/python/api/types/common) | Runtime reference; .NET equivalents and differences above |
| [Exceptions](https://docs.typesafe.ai/sdk/python/api/exceptions) | Runtime reference; .NET equivalents and differences above |
| [Constants](https://docs.typesafe.ai/sdk/python/api/constants) | Runtime reference; .NET equivalents and differences above |
| [JavaScript SDK](https://docs.typesafe.ai/sdk/javascript) | Runtime reference; .NET equivalents and differences above |
| [Changelog](https://docs.typesafe.ai/sdk/javascript/changelog) | Runtime reference; .NET equivalents and differences above |
| [API reference](https://docs.typesafe.ai/sdk/javascript/api) | Runtime reference; .NET equivalents and differences above |
| [Class: APIConnectionError](https://docs.typesafe.ai/sdk/javascript/api/classes/APIConnectionError) | Runtime reference; .NET equivalents and differences above |
| [Class: APIError](https://docs.typesafe.ai/sdk/javascript/api/classes/APIError) | Runtime reference; .NET equivalents and differences above |
| [Class: APIPromise<T>](https://docs.typesafe.ai/sdk/javascript/api/classes/APIPromise) | Runtime reference; .NET equivalents and differences above |
| [Class: APITimeoutError](https://docs.typesafe.ai/sdk/javascript/api/classes/APITimeoutError) | Runtime reference; .NET equivalents and differences above |
| [Class: APIUserAbortError](https://docs.typesafe.ai/sdk/javascript/api/classes/APIUserAbortError) | Runtime reference; .NET equivalents and differences above |
| [Class: AuthenticationError](https://docs.typesafe.ai/sdk/javascript/api/classes/AuthenticationError) | Runtime reference; .NET equivalents and differences above |
| [Class: BadRequestError](https://docs.typesafe.ai/sdk/javascript/api/classes/BadRequestError) | Runtime reference; .NET equivalents and differences above |
| [Class: Image](https://docs.typesafe.ai/sdk/javascript/api/classes/Image) | Runtime reference; .NET equivalents and differences above |
| [Class: InternalServerError](https://docs.typesafe.ai/sdk/javascript/api/classes/InternalServerError) | Runtime reference; .NET equivalents and differences above |
| [Class: NotFoundError](https://docs.typesafe.ai/sdk/javascript/api/classes/NotFoundError) | Runtime reference; .NET equivalents and differences above |
| [Class: PermissionDeniedError](https://docs.typesafe.ai/sdk/javascript/api/classes/PermissionDeniedError) | Runtime reference; .NET equivalents and differences above |
| [Class: RateLimitError](https://docs.typesafe.ai/sdk/javascript/api/classes/RateLimitError) | Runtime reference; .NET equivalents and differences above |
| [Class: TypeSafeClient](https://docs.typesafe.ai/sdk/javascript/api/classes/TypeSafeClient) | Runtime reference; .NET equivalents and differences above |
| [Class: TypeSafeError](https://docs.typesafe.ai/sdk/javascript/api/classes/TypeSafeError) | Runtime reference; .NET equivalents and differences above |
| [Class: UnprocessableEntityError](https://docs.typesafe.ai/sdk/javascript/api/classes/UnprocessableEntityError) | Runtime reference; .NET equivalents and differences above |
| [Interface: BoxCoordinates](https://docs.typesafe.ai/sdk/javascript/api/interfaces/BoxCoordinates) | Runtime reference; .NET equivalents and differences above |
| [Interface: BoxQuestion](https://docs.typesafe.ai/sdk/javascript/api/interfaces/BoxQuestion) | Runtime reference; .NET equivalents and differences above |
| [Interface: BoxResponse](https://docs.typesafe.ai/sdk/javascript/api/interfaces/BoxResponse) | Runtime reference; .NET equivalents and differences above |
| [Interface: ChoiceQuestion<T>](https://docs.typesafe.ai/sdk/javascript/api/interfaces/ChoiceQuestion) | Runtime reference; .NET equivalents and differences above |
| [Interface: ChoiceResponse<T>](https://docs.typesafe.ai/sdk/javascript/api/interfaces/ChoiceResponse) | Runtime reference; .NET equivalents and differences above |
| [Interface: Logger](https://docs.typesafe.ai/sdk/javascript/api/interfaces/Logger) | Runtime reference; .NET equivalents and differences above |
| [Interface: ModelCard](https://docs.typesafe.ai/sdk/javascript/api/interfaces/ModelCard) | Runtime reference; .NET equivalents and differences above |
| [Interface: Models](https://docs.typesafe.ai/sdk/javascript/api/interfaces/Models) | Runtime reference; .NET equivalents and differences above |
| [Interface: NoulQuestion](https://docs.typesafe.ai/sdk/javascript/api/interfaces/NoulQuestion) | Runtime reference; .NET equivalents and differences above |
| [Interface: NoulResponse](https://docs.typesafe.ai/sdk/javascript/api/interfaces/NoulResponse) | Runtime reference; .NET equivalents and differences above |
| [Interface: Questions](https://docs.typesafe.ai/sdk/javascript/api/interfaces/Questions) | Runtime reference; .NET equivalents and differences above |
| [Interface: RequestOptions](https://docs.typesafe.ai/sdk/javascript/api/interfaces/RequestOptions) | Runtime reference; .NET equivalents and differences above |
| [Interface: RetryPolicy](https://docs.typesafe.ai/sdk/javascript/api/interfaces/RetryPolicy) | Runtime reference; .NET equivalents and differences above |
| [Interface: ScoreQuestion<T>](https://docs.typesafe.ai/sdk/javascript/api/interfaces/ScoreQuestion) | Runtime reference; .NET equivalents and differences above |
| [Interface: ScoreResponse<T>](https://docs.typesafe.ai/sdk/javascript/api/interfaces/ScoreResponse) | Runtime reference; .NET equivalents and differences above |
| [Interface: SystemOneRequest<Q>](https://docs.typesafe.ai/sdk/javascript/api/interfaces/SystemOneRequest) | Runtime reference; .NET equivalents and differences above |
| [Interface: SystemOneResult<Q>](https://docs.typesafe.ai/sdk/javascript/api/interfaces/SystemOneResult) | Runtime reference; .NET equivalents and differences above |
| [Interface: TextWithAssets](https://docs.typesafe.ai/sdk/javascript/api/interfaces/TextWithAssets) | Runtime reference; .NET equivalents and differences above |
| [Interface: TypeSafeClientConfig](https://docs.typesafe.ai/sdk/javascript/api/interfaces/TypeSafeClientConfig) | Runtime reference; .NET equivalents and differences above |
| [Interface: Usage](https://docs.typesafe.ai/sdk/javascript/api/interfaces/Usage) | Runtime reference; .NET equivalents and differences above |
| [Interface: WithResponse<T>](https://docs.typesafe.ai/sdk/javascript/api/interfaces/WithResponse) | Runtime reference; .NET equivalents and differences above |
| [Type Alias: Asset](https://docs.typesafe.ai/sdk/javascript/api/type-aliases/Asset) | Runtime reference; .NET equivalents and differences above |
| [Type Alias: ChoiceCriteria](https://docs.typesafe.ai/sdk/javascript/api/type-aliases/ChoiceCriteria) | Runtime reference; .NET equivalents and differences above |
| [Type Alias: Compression](https://docs.typesafe.ai/sdk/javascript/api/type-aliases/Compression) | Runtime reference; .NET equivalents and differences above |
| [Type Alias: ContentValue](https://docs.typesafe.ai/sdk/javascript/api/type-aliases/ContentValue) | Runtime reference; .NET equivalents and differences above |
| [Type Alias: Description](https://docs.typesafe.ai/sdk/javascript/api/type-aliases/Description) | Runtime reference; .NET equivalents and differences above |
| [Type Alias: EntryType](https://docs.typesafe.ai/sdk/javascript/api/type-aliases/EntryType) | Runtime reference; .NET equivalents and differences above |
| [Type Alias: EnvVar](https://docs.typesafe.ai/sdk/javascript/api/type-aliases/EnvVar) | Runtime reference; .NET equivalents and differences above |
| [Type Alias: Fetch](https://docs.typesafe.ai/sdk/javascript/api/type-aliases/Fetch) | Runtime reference; .NET equivalents and differences above |
| [Type Alias: JsonValue](https://docs.typesafe.ai/sdk/javascript/api/type-aliases/JsonValue) | Runtime reference; .NET equivalents and differences above |
| [Type Alias: LogLevel](https://docs.typesafe.ai/sdk/javascript/api/type-aliases/LogLevel) | Runtime reference; .NET equivalents and differences above |
| [Type Alias: Question](https://docs.typesafe.ai/sdk/javascript/api/type-aliases/Question) | Runtime reference; .NET equivalents and differences above |
| [Type Alias: ResultFor<T>](https://docs.typesafe.ai/sdk/javascript/api/type-aliases/ResultFor) | Runtime reference; .NET equivalents and differences above |
| [Type Alias: ScoreCriteria](https://docs.typesafe.ai/sdk/javascript/api/type-aliases/ScoreCriteria) | Runtime reference; .NET equivalents and differences above |
| [Type Alias: ScoreLegend<T>](https://docs.typesafe.ai/sdk/javascript/api/type-aliases/ScoreLegend) | Runtime reference; .NET equivalents and differences above |
| [Type Alias: ScoreOf<T>](https://docs.typesafe.ai/sdk/javascript/api/type-aliases/ScoreOf) | Runtime reference; .NET equivalents and differences above |
| [Type Alias: SentContent<T>](https://docs.typesafe.ai/sdk/javascript/api/type-aliases/SentContent) | Runtime reference; .NET equivalents and differences above |
| [Variable: ENV](https://docs.typesafe.ai/sdk/javascript/api/variables/ENV) | Runtime reference; .NET equivalents and differences above |
| [Variable: LOG_LEVELS](https://docs.typesafe.ai/sdk/javascript/api/variables/LOG_LEVELS) | Runtime reference; .NET equivalents and differences above |
| [Variable: noul](https://docs.typesafe.ai/sdk/javascript/api/variables/noul) | Runtime reference; .NET equivalents and differences above |
| [Variable: score](https://docs.typesafe.ai/sdk/javascript/api/variables/score) | Runtime reference; .NET equivalents and differences above |
| [Variable: VERSION](https://docs.typesafe.ai/sdk/javascript/api/variables/VERSION) | Runtime reference; .NET equivalents and differences above |
| [Variable: withAssets](https://docs.typesafe.ai/sdk/javascript/api/variables/withAssets) | Runtime reference; .NET equivalents and differences above |
| [Function: box()](https://docs.typesafe.ai/sdk/javascript/api/functions/box) | Runtime reference; .NET equivalents and differences above |
| [Function: choice()](https://docs.typesafe.ai/sdk/javascript/api/functions/choice) | Runtime reference; .NET equivalents and differences above |
| [Function: image()](https://docs.typesafe.ai/sdk/javascript/api/functions/image) | Runtime reference; .NET equivalents and differences above |

Index snapshot SHA-256: `99f746d2570aa97898ad0e62042b9b5b703e54c3f1142ce4cf4b7ff15b5782ae`.
