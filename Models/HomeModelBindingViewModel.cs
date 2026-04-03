namespace Ceng382_25_26_202211035.Models;

public record HomeModelBindingViewModel(
    Thing Thing,
    bool HasErrors,
    IEnumerable<string> ValidationErrors);
    