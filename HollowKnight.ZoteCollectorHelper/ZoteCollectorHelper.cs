namespace ZoteSummonNoLimit;
[Serializable]
public class Settings
{
    public bool on = true;
    public bool zoteBossSkin = true;
    public int zoteSummonHP = 57;
    public int zoteSummonLimit = 3;
    public int zoteScale = 4;
    public int colSummonHP = 35;
}
public class ZoteCollectorHelper : Mod, IGlobalSettings<Settings>, IMenuMod
{
    private Settings settings_ = new();
    private Texture2D zoteBossSkin;
    private Texture zoteBossSkinOld;
    private List<GameObject> activeZotelings = new List<GameObject>();
    public bool ToggleButtonInsideMenu => true;
    public ZoteCollectorHelper() : base("ZoteCollectorHelper")
    {
    }
    public override string GetVersion() => "1.0.0.0";
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        var stream = typeof(ZoteCollectorHelper).Assembly.GetManifestResourceStream("ZoteCollectorHelper.Grey Prince.png");
        MemoryStream memoryStream = new((int)stream.Length);
        stream.CopyTo(memoryStream);
        stream.Close();
        var bytes = memoryStream.ToArray();
        memoryStream.Close();
        zoteBossSkin = new(0, 0);
        zoteBossSkin.LoadImage(bytes, true);
    }
    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        if (arg1.name == "GG_Grey_Prince_Zote")
        {
            Log("Found scene " + arg1.name);
            foreach (var g in arg1.GetAllGameObjects())
            {
                if (g.name == "Grey Prince" || g.name.StartsWith("Zoteling") || g.name.StartsWith("Zote Balloon"))
                {
                    Log("Found object " + g.name);
                    var s = g.transform.localScale;
                    s.x = settings_.zoteScale / 4.0f * Math.Sign(s.x);
                    s.y = settings_.zoteScale / 4.0f * Math.Sign(s.y);
                    g.transform.localScale = s;
                    if (g.name == "Grey Prince")
                    {
                        var f = g.LocateMyFSM("Control");
                        f.AddCustomAction("Roar End", () =>
                        {
                            var s = g.transform.localScale;
                            s.x = settings_.zoteScale / 4.0f * Math.Sign(s.x);
                            s.y = settings_.zoteScale / 4.0f * Math.Sign(s.y);
                            g.transform.localScale = s;
                        });
                        f.InsertCustomAction("Charge L", () =>
                        {
                            var s = g.transform.localScale;
                            s.x = settings_.zoteScale / 4.0f * Math.Sign(s.x);
                            s.y = settings_.zoteScale / 4.0f * Math.Sign(s.y);
                            g.transform.localScale = s;
                        }, 2);
                        f.InsertCustomAction("Charge R", () =>
                        {
                            var s = g.transform.localScale;
                            s.x = settings_.zoteScale / 4.0f * Math.Sign(s.x);
                            s.y = settings_.zoteScale / 4.0f * Math.Sign(s.y);
                            g.transform.localScale = s;
                        }, 2);
                        foreach (var state in f.FsmStates)
                        {
                            f.AddCustomAction(state.Name, () =>
                            {
                                Log(f.ActiveStateName + " " + g.transform.localScale.ToString());
                            });
                        }
                    }
                    if (g.name.StartsWith("Zote Balloon"))
                    {
                        var f = g.LocateMyFSM("Control");
                        f.InsertCustomAction("Set Pos", () =>
                        {
                            var s = g.transform.localScale;
                            s.x = settings_.zoteScale / 4.0f * Math.Sign(s.x);
                            s.y = settings_.zoteScale / 4.0f * Math.Sign(s.y);
                            g.transform.localScale = s;
                        }, 4);
                    }
                }
            }
        }
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
                    zoteBossSkinOld = tk2dSprite.CurrentSprite.material.mainTexture;
                    tk2dSprite.CurrentSprite.material.mainTexture = zoteBossSkin;
                }
                else if(zoteBossSkinOld)
                {
                    var tk2dSprite = self.gameObject.GetComponent<tk2dSprite>();
                    tk2dSprite.CurrentSprite.material.mainTexture = zoteBossSkinOld;
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
        if (self.gameObject.scene.name == "GG_Collector_V" && self.gameObject.name == "Jar Collector")
        {
            int hp = settings_.on ? settings_.colSummonHP : 26;
            self.AccessIntVariable("Buzzer HP").Value = hp;
            self.AccessIntVariable("Roller HP").Value = hp;
            self.AccessIntVariable("Spitter HP").Value = hp;
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
        menus.Add(
           new()
           {
               Name="Zote Scale",
               Values = new string[]
               {
                    "0.25",
                    "0.5",
                    "0.75",
                    "1",
                    "1.25",
                    "1.5",
                    "1.75",
                    "2"
               },
               Saver = i => settings_.zoteScale = i + 1,
               Loader = () => settings_.zoteScale - 1
           }
        );
        menus.Add(
            new()
            {
                Name = "Collector Summon HP",
                Values = Enumerable.Range(0, 100).Select(n => n.ToString()).ToArray(),
                Saver = i => settings_.colSummonHP = i,
                Loader = () => settings_.colSummonHP
            }
        );
        return menus;
    }
}
