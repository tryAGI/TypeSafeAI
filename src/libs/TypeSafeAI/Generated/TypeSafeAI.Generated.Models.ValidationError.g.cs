
#nullable enable

namespace TypeSafeAI.Generated
{
    /// <summary>
    /// A request validation error at a specific field or array element.
    /// </summary>
    public sealed partial class ValidationError
    {
        /// <summary>
        /// Path to the invalid value: the request location followed by field names and array indices.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("loc")]
        [global::System.Text.Json.Serialization.JsonRequired]
        public required global::System.Collections.Generic.IList<global::TypeSafeAI.Generated.AnyOf<string, int?>> Loc { get; set; }

        /// <summary>
        /// Human-readable explanation of the validation failure.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("msg")]
        [global::System.Text.Json.Serialization.JsonRequired]
        public required string Msg { get; set; }

        /// <summary>
        /// Machine-readable validation error code.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("type")]
        [global::System.Text.Json.Serialization.JsonRequired]
        public required string Type { get; set; }

        /// <summary>
        /// The input value that failed validation.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("input")]
        public object? Input { get; set; }

        /// <summary>
        /// Additional context used to explain the validation failure.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("ctx")]
        public object? Ctx { get; set; }

        /// <summary>
        /// Raw JSON properties that are not explicitly defined in the schema
        /// </summary>
        [global::System.Text.Json.Serialization.JsonExtensionData]
        public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement> AdditionalProperties { get; set; } = new global::System.Collections.Generic.Dictionary<string, global::System.Text.Json.JsonElement>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationError" /> class.
        /// </summary>
        /// <param name="loc">
        /// Path to the invalid value: the request location followed by field names and array indices.
        /// </param>
        /// <param name="msg">
        /// Human-readable explanation of the validation failure.
        /// </param>
        /// <param name="type">
        /// Machine-readable validation error code.
        /// </param>
        /// <param name="input">
        /// The input value that failed validation.
        /// </param>
        /// <param name="ctx">
        /// Additional context used to explain the validation failure.
        /// </param>
#if NET7_0_OR_GREATER
        [global::System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
#endif
        public ValidationError(
            global::System.Collections.Generic.IList<global::TypeSafeAI.Generated.AnyOf<string, int?>> loc,
            string msg,
            string type,
            object? input,
            object? ctx)
        {
            this.Loc = loc ?? throw new global::System.ArgumentNullException(nameof(loc));
            this.Msg = msg ?? throw new global::System.ArgumentNullException(nameof(msg));
            this.Type = type ?? throw new global::System.ArgumentNullException(nameof(type));
            this.Input = input;
            this.Ctx = ctx;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationError" /> class.
        /// </summary>
        public ValidationError()
        {
        }

    }
}