namespace ZoteSummonNoLimit;
[Serializable]
public class Settings
{
    public bool on = true;
    public int zoteSummonHP = 57;
}
public class ZoteCollectorHelper : Mod, IGlobalSettings<Settings>, IMenuMod
{
    public Settings settings_ = new();
    public bool ToggleButtonInsideMenu => true;
    public ZoteCollectorHelper() : base("ZoteCollectorHelper")
    {
    }
    public override string GetVersion() => "1.0.0.0";
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
    }
    private void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        if (settings_.on)
        {
            if (self.gameObject.scene.name == "GG_Grey_Prince_Zote" && self.gameObject.name == "Grey Prince" && self.FsmName == "Control")
            {
            }
            else if (self.gameObject.scene.name == "GG_Grey_Prince_Zote" && self.gameObject.name.StartsWith("Zoteling") && self.FsmName == "Control")
            {
                Log("Adding destroy.");
                self.GetState("Choice").InsertCustomAction(() =>
                {
                    self.gameObject.GetComponent<HealthManager>().hp = settings_.zoteSummonHP;
                }, 0);
                Log("Added destroy.");
            }
        }
        orig(self);
    }
    public void OnLoadGlobal(Settings settings) => settings_ = settings;
    public Settings OnSaveGlobal() => settings_;
    public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? menu)
    {
        List<IMenuMod.MenuEntry> menus = new();
        menus.Add(
            new()
            {
                Values = new string[]
                {
                    Language.Language.Get("MOH_ON", "MainMenu"),
                    Language.Language.Get("MOH_OFF", "MainMenu"),
                },
                Saver = i => settings_.on = i == 0,
                Loader = () => settings_.on ? 0 : 1
            }
        );
        menus.Add(
            new()
            {
                Name = "Zote Summon HP",
                Values = Enumerable.Range(0, 100).Select(n => n.ToString()).ToArray(),
                Saver = i => settings_.zoteSummonHP = i,
                Loader = () => settings_.zoteSummonHP
            }
        );
        return menus;
    }
}
