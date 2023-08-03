namespace ZoteSummonNoLimit;
[Serializable]
public class Settings
{
    public bool on = true;
    public bool zoteStatueSkin = true;
    public bool zoteBossSkin = true;
    public int zoteSummonHP = 57;
    public int zoteSummonLimit = 3;
}
public class ZoteCollectorHelper : Mod, IGlobalSettings<Settings>, IMenuMod
{
    private Settings settings_ = new();
    private Texture2D zoteStatueSkin;
    private Texture2D zoteBossSkin;
    private List<GameObject> activeZotelings = new List<GameObject>();
    public bool ToggleButtonInsideMenu => true;
    public ZoteCollectorHelper() : base("ZoteCollectorHelper")
    {
    }
    public override string GetVersion() => "1.0.0.0";
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
        var stream = typeof(ZoteCollectorHelper).Assembly.GetManifestResourceStream("ZoteCollectorHelper.GG_Statue_GreyPrince.png");
        MemoryStream memoryStream = new((int)stream.Length);
        stream.CopyTo(memoryStream);
        stream.Close();
        var bytes = memoryStream.ToArray();
        memoryStream.Close();
        zoteStatueSkin = new(0, 0);
        zoteStatueSkin.LoadImage(bytes, true);
        stream = typeof(ZoteCollectorHelper).Assembly.GetManifestResourceStream("ZoteCollectorHelper.Grey Prince.png");
        memoryStream = new((int)stream.Length);
        stream.CopyTo(memoryStream);
        stream.Close();
        bytes = memoryStream.ToArray();
        memoryStream.Close();
        zoteBossSkin = new(0, 0);
        zoteBossSkin.LoadImage(bytes, true);
    }
    private void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        if (settings_.on)
        {
            if (self.gameObject.scene.name == "GG_Grey_Prince_Zote" && self.gameObject.name == "Grey Prince" && self.FsmName == "Control")
            {
                self.GetState("Spit Antic").RemoveAction(3);
                self.GetState("Spit Antic").InsertCustomAction(() =>
                {
                    List<GameObject>remaining=new List<GameObject>();
                    foreach(var z in activeZotelings)
                    {
                        if (z != null)
                        {
                            remaining.Add(z);
                        }
                    }
                    activeZotelings = remaining;
                    Log("Current no zotelings: " + activeZotelings.Count.ToString());
                    if (activeZotelings.Count >= settings_.zoteSummonLimit)
                    {
                        Log("Reached limit.");
                        self.SendEvent("CANCEL");
                    }
                }, 0);
                void summon()
                {
                    var zoteling = self.AccessGameObjectVariable("Zoteling").Value;
                    var newZoteling = UnityEngine.Object.Instantiate(zoteling);
                    self.AccessGameObjectVariable("Zoteling").Value = newZoteling;
                    activeZotelings.Add(newZoteling);
                }
                self.GetState("Spit L").InsertCustomAction(summon, 1);
                self.GetState("Spit R").InsertCustomAction(summon, 1);
                if (settings_.zoteBossSkin)
                {
                    var tk2dSprite = self.gameObject.GetComponent<tk2dSprite>();
                    tk2dSprite.CurrentSprite.material.mainTexture = zoteBossSkin;
                }
            }
            else if (self.gameObject.scene.name == "GG_Grey_Prince_Zote" && self.gameObject.name.StartsWith("Zoteling") && self.FsmName == "Control")
            {
                self.GetState("Choice").InsertCustomAction(() =>
                {
                    self.gameObject.GetComponent<HealthManager>().hp = settings_.zoteSummonHP;
                }, 0);
                self.GetState("Reset").AddCustomAction(() =>
                {
                    UnityEngine.Object.Destroy(self.gameObject);
                });
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
                Name="Zote Statue Skin",
                Values = new string[]
                {
                    Language.Language.Get("MOH_ON", "MainMenu"),
                    Language.Language.Get("MOH_OFF", "MainMenu"),
                },
                Saver = i => settings_.zoteStatueSkin = i == 0,
                Loader = () => settings_.zoteStatueSkin ? 0 : 1
            }
        );
         menus.Add(
            new()
            {
                Name="Zote Boss Skin",
                Values = new string[]
                {
                    Language.Language.Get("MOH_ON", "MainMenu"),
                    Language.Language.Get("MOH_OFF", "MainMenu"),
                },
                Saver = i => settings_.zoteBossSkin = i == 0,
                Loader = () => settings_.zoteBossSkin ? 0 : 1
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
        menus.Add(
            new()
            {
                Name = "Zote Summon Limit",
                Values = Enumerable.Range(0, 100).Select(n => n.ToString()).ToArray(),
                Saver = i => settings_.zoteSummonLimit = i,
                Loader = () => settings_.zoteSummonLimit
            }
        );
        return menus;
    }
}
