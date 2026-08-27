// Settings schema (POCO). Serialized by Newtonsoft.Json into setting.json,
// alongside but separate from save.json / SaveData (game progress).
// Field defaults MUST match the previous hard-coded fallbacks so that a
// missing setting.json behaves exactly like a fresh install did before.

public class SettingsData
{
    // Bump only when field meaning changes.
    public int SettingsVersion = 1;

    // Game tab. Language: 0 = Korean, 1 = English.
    public int  Language     = 1;
    public bool ScreenShake  = true;
    public bool TutorialHint = true;

    // Graphics tab. Index into GraphicsSettingsPanel.ScreenModes.
    public int ScreenMode = 0;

    // Sound tab. Linear 0..1 slider values.
    public float BgmVolume = 1f;
    public float SfxVolume = 1f;

    // Controls tab. Raw output of InputActionAsset.SaveBindingOverridesAsJson().
    // Empty string means "no rebinding applied".
    public string InputBindingsJson = "";
}
