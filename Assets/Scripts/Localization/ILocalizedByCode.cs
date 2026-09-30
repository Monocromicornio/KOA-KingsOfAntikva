/// <summary>
/// Marks a component that writes its own texts (and its children's texts) from code using the localization table.
/// The localization scene scanner never adds a LocalizedText under these components, so both never fight over the same text.
/// </summary>
public interface ILocalizedByCode
{
}
