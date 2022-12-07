// File:    Valweed.cs
// Project: Valweed
using BepInEx;
using BepInEx.Configuration;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System.Reflection;
using UnityEngine;
using HarmonyLib;
using System;

namespace Valweed
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class Valweed : BaseUnityPlugin
    {
        public const string PluginGUID = "com.drod917.Valweed";
        public const string PluginName = "Valweed";
        public const string PluginVersion = "0.3.4";

        private Harmony harmony = new Harmony("com.drod917.Valweed");

        private AssetBundle jointResourceBundle;
        private AssetBundle plantResourceBundle;
        private AssetBundle bongResourceBundle;
        private AssetBundle spriteBundle;

        private GameObject hybridJointPrefab;
        private GameObject indicaJointPrefab;
        private GameObject sativaJointPrefab;

        private GameObject weedSaplingPrefab;
        private GameObject weedSeedSaplingPrefab;
        private GameObject pickableWeedPrefab;
        private GameObject pickableSeedWeedPrefab;

        private GameObject weedNugsPrefab;
        private GameObject weedNugPrefab;
        private GameObject weedSeedsPrefab;
        private GameObject weedPaperPrefab;

        private GameObject bongPrefab;
        private GameObject bongNoisePrefab;
        private GameObject bongSmokePrefab;
        private GameObject bongMouthSmokePrefab;
        private GameObject bongInhaleNoisePrefab;

        private GameObject mouthSmokePrefab;
        private GameObject jointNoisePrefab;

        private CustomStatusEffect hybridJointEffect;
        private CustomStatusEffect indicaJointEffect;
        private CustomStatusEffect sativaJointEffect;
        private CustomStatusEffect bongStatusEffect;

        // Buff Settings
        private int jointHealthRegenVal;
        private int jointStamRegenVal;
        private float jointEffectTime;
        private bool cosmeticOnly;

        // Yield Settings
        private int seedGrowTime;
        private int seedYield;
        private int seedYieldBonus;
        private float seedYieldBonusChance;
        private int budGrowTime;
        private int budYield;
        private int budYieldBonus;
        private float budYieldBonusChance;

        private void Awake()
        {
            initConfig();
            LoadAssets();
            AddStatusEffects();
            CreateItems();
            CreatePieces();
            harmony.PatchAll(typeof(PlantHaveRoof));
            harmony.PatchAll(typeof(PlayerCanConsumeItem));

            Config.SaveOnConfigSet = true;
            Config.SettingChanged += (sender, args) => SettingChanged(Config, args);
        }

        private void initConfig()
        {
            jointHealthRegenVal = Config.Bind("Buff Settings", "Joint Health Regen Rate", 50,
                new ConfigDescription("Percent rate to increase health regen (Default 50 (+50%))",
                new AcceptableValueRange<int>(0, 1000),
                new ConfigurationManagerAttributes { IsAdminOnly = true }
                )).Value;

            jointStamRegenVal = Config.Bind("Buff Settings", "Joint Stamina Regen Rate", 100,
                new ConfigDescription("Percent rate to increase stamina regen (Default 100 (+100%))",
                new AcceptableValueRange<int>(0, 1000),
                new ConfigurationManagerAttributes { IsAdminOnly = true }
                )).Value;

            jointEffectTime = Config.Bind("Buff Settings", "Joint Buff Duration", 600,
                new ConfigDescription("Effective time for smoking to last (Default 600s (10m))",
                new AcceptableValueRange<int>(20, 3600),
                new ConfigurationManagerAttributes { IsAdminOnly = true }
                )).Value;

            cosmeticOnly = Config.Bind("Buff Settings", "Cosmetic Effects Only", false,
                new ConfigDescription("Disables health and stamina regen modifiers. (Default Off)",
                acceptableValues: null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }
                )).Value;

            seedGrowTime = Config.Bind("Yield Settings", "Seed Grow Time", 4000,
                new ConfigDescription("Adjusts how long until seeds can be harvested. (Default 4000)",
                new AcceptableValueRange<int>(10, 16000),
                new ConfigurationManagerAttributes { IsAdminOnly = true }
                )).Value;
            seedYield = Config.Bind("Yield Settings", "Seed Yield", 2,
                new ConfigDescription("Adjusts how many seeds are harvested. (Default 2)",
                new AcceptableValueRange<int>(1, 99),
                new ConfigurationManagerAttributes { IsAdminOnly = true }
                )).Value;
            seedYieldBonus = Config.Bind("Yield Settings", "Seed Yield Bonus", 1,
                new ConfigDescription("Adjusts how many bonus seeds are harvested. (Default 1)",
                new AcceptableValueRange<int>(1, 99),
                new ConfigurationManagerAttributes { IsAdminOnly = true }
                )).Value;
            seedYieldBonusChance = Config.Bind("Yield Settings", "Seed Yield Bonus Chance", 75,
                new ConfigDescription("Adjusts the chance to harvest bonus seeds. (Default 75%)",
                new AcceptableValueRange<int>(1, 100),
                new ConfigurationManagerAttributes { IsAdminOnly = true }
                )).Value / 100f;

            budGrowTime = Config.Bind("Yield Settings", "Bud Grow Time", 4000,
                new ConfigDescription("Adjusts how long until buds can be harvested. (Default 4000)",
                new AcceptableValueRange<int>(10, 16000),
                new ConfigurationManagerAttributes { IsAdminOnly = true }
                )).Value;
            budYield = Config.Bind("Yield Settings", "Bud Yield", 1,
                new ConfigDescription("Adjusts how many buds are harvested. (Default 1)",
                new AcceptableValueRange<int>(1, 99),
                new ConfigurationManagerAttributes { IsAdminOnly = true }
                )).Value;
            budYieldBonus = Config.Bind("Yield Settings", "Bud Yield Bonus", 2,
                new ConfigDescription("Adjusts how many bonus buds are harvested. (Default 2)",
                new AcceptableValueRange<int>(1, 99),
                new ConfigurationManagerAttributes { IsAdminOnly = true }
                )).Value;
            budYieldBonusChance = Config.Bind("Yield Settings", "Bud Yield Bonus Chance", 75,
                new ConfigDescription("Adjusts the chance to harvest bonus buds. (Default 75%)",
                new AcceptableValueRange<int>(1, 100),
                new ConfigurationManagerAttributes { IsAdminOnly = true }
                )).Value / 100f;

        }



        private void SettingChanged(ConfigFile sender, SettingChangedEventArgs args)
        {
            initConfig();

            string replaceString;

            Bong bong = PrefabManager.Instance.GetPrefab("Bong").GetComponent<Bong>();
            bong.cosmeticOnly = cosmeticOnly;

            // RE-set drop rates according to config
            Plant saplingWeed = PrefabManager.Instance.GetPrefab("sapling_weed").GetComponent<Plant>();
            saplingWeed.m_growTime = budGrowTime;
            saplingWeed.m_growTimeMax = budGrowTime;
            saplingWeed.m_biome = (Heightmap.Biome)25;

            Pickable pickableWeed = PrefabManager.Instance.GetPrefab("Pickable_WeedPlant").GetComponent<Pickable>();
            pickableWeed.m_amount = budYield;
            pickableWeed.m_extraDrops.m_dropMin = budYieldBonus;
            pickableWeed.m_extraDrops.m_dropMax = budYieldBonus;
            pickableWeed.m_extraDrops.m_dropChance = budYieldBonusChance;

            Plant saplingWeedSeed = PrefabManager.Instance.GetPrefab("sapling_seedweed").GetComponent<Plant>();
            saplingWeedSeed.m_growTime = seedGrowTime;
            saplingWeedSeed.m_growTimeMax = seedGrowTime;
            saplingWeedSeed.m_biome = (Heightmap.Biome)25;

            Pickable pickableSeedWeed = PrefabManager.Instance.GetPrefab("Pickable_SeedWeedPlant").GetComponent<Pickable>();
            pickableSeedWeed.m_amount = seedYield;
            pickableSeedWeed.m_extraDrops.m_dropMin = seedYieldBonus;
            pickableSeedWeed.m_extraDrops.m_dropMax = seedYieldBonus;
            pickableSeedWeed.m_extraDrops.m_dropChance = seedYieldBonusChance;

            //SE_Sativa seSativa = FindObjectOfType<SE_Sativa>();
            SE_Sativa seSativa = ObjectDB.FindObjectOfType<SE_Sativa>();
            seSativa.cosmeticOnly = cosmeticOnly;
            seSativa.healthRegenVal = jointHealthRegenVal;
            seSativa.staminaRegenVal = jointStamRegenVal;
            seSativa.ttl = jointEffectTime;
            replaceString = "$sativa_joint_tooltip";
            string sativaJointTooltip = $"{replaceString}{(cosmeticOnly ? "" : "\n\nHunger Rate -50%")}";
            seSativa.m_tooltip = sativaJointTooltip;

            SE_Indica seIndica = ObjectDB.FindObjectOfType<SE_Indica>();
            seIndica.cosmeticOnly = cosmeticOnly;
            seIndica.healthRegenVal = jointHealthRegenVal;
            seIndica.staminaRegenVal = jointStamRegenVal;
            seIndica.ttl = jointEffectTime;
            replaceString = "$indica_joint_tooltip";
            string indicaJointTooltip = $"{replaceString}";
            seIndica.m_tooltip = indicaJointTooltip;

            SE_Hybrid seHybrid = ObjectDB.FindObjectOfType<SE_Hybrid>();
            seHybrid.cosmeticOnly = cosmeticOnly;
            seHybrid.healthRegenVal = jointHealthRegenVal;
            seHybrid.staminaRegenVal = jointStamRegenVal;
            seHybrid.ttl = jointEffectTime;
            replaceString = "$hybrid_joint_tooltip";
            string hybridJointTooltip = $"{replaceString}";
            seHybrid.m_tooltip = hybridJointTooltip;

            SE_Bong seBong = ObjectDB.FindObjectOfType<SE_Bong>();
            seBong.cosmeticOnly = cosmeticOnly;
            seBong.healthRegenVal = jointHealthRegenVal;
            seBong.staminaRegenVal = jointStamRegenVal;
            seBong.ttl = jointEffectTime * 3;
            replaceString = "$bong_tooltip";
            string bongTooltip = $"{replaceString}{(cosmeticOnly ? "" : "\nHunger Rate -50%")}";
            seBong.m_tooltip = bongTooltip;

        }

        private void Update()
        {
        }

        private void LoadAssets()
        {
            //Load embedded resources
            //Jotunn.Logger.LogInfo($"Embedded resources: {string.Join(",", Assembly.GetExecutingAssembly().GetManifestResourceNames())}");
            // Joint stuff
            jointResourceBundle = AssetUtils.LoadAssetBundleFromResources("joint", Assembly.GetExecutingAssembly());
            hybridJointPrefab = jointResourceBundle.LoadAsset<GameObject>("JointHybrid");
            indicaJointPrefab = jointResourceBundle.LoadAsset<GameObject>("JointIndica");
            sativaJointPrefab = jointResourceBundle.LoadAsset<GameObject>("JointSativa");
            weedPaperPrefab = jointResourceBundle.LoadAsset<GameObject>("JointPaper");
            jointNoisePrefab = jointResourceBundle.LoadAsset<GameObject>("sfx_hit_joint");

            // Growable stuff
            plantResourceBundle = AssetUtils.LoadAssetBundleFromResources("plant", Assembly.GetExecutingAssembly());
            weedSaplingPrefab = plantResourceBundle.LoadAsset<GameObject>("sapling_weed");
            weedSeedSaplingPrefab = plantResourceBundle.LoadAsset<GameObject>("sapling_seedweed");

            pickableWeedPrefab = plantResourceBundle.LoadAsset<GameObject>("Pickable_WeedPlant");
            pickableSeedWeedPrefab = plantResourceBundle.LoadAsset<GameObject>("Pickable_SeedWeedPlant");

            // ItemDrop stuff
            weedNugsPrefab = plantResourceBundle.LoadAsset<GameObject>("WeedBuds");
            weedSeedsPrefab = plantResourceBundle.LoadAsset<GameObject>("WeedSeeds");

            bongResourceBundle = AssetUtils.LoadAssetBundleFromResources("bong", Assembly.GetExecutingAssembly());
            bongPrefab = bongResourceBundle.LoadAsset<GameObject>("Bong");
            weedNugPrefab = bongResourceBundle.LoadAsset<GameObject>("bud");
            bongNoisePrefab = bongResourceBundle.LoadAsset<GameObject>("sfx_hit_bong");
            bongInhaleNoisePrefab = bongResourceBundle.LoadAsset<GameObject>("sfx_hit_bong_inhale");
            bongSmokePrefab = bongResourceBundle.LoadAsset<GameObject>("vfx_bong_smoke");
            mouthSmokePrefab = bongResourceBundle.LoadAsset<GameObject>("vfx_mouth_smoke");
            bongMouthSmokePrefab = bongResourceBundle.LoadAsset<GameObject>("vfx_mouth_smoke_bong");

            // Set drop rates according to config
            weedSaplingPrefab.GetComponent<Plant>().m_growTime = budGrowTime;
            weedSaplingPrefab.GetComponent<Plant>().m_growTimeMax = budGrowTime;
            weedSaplingPrefab.GetComponent<Plant>().m_biome = (Heightmap.Biome)25;
            pickableWeedPrefab.GetComponent<Pickable>().m_amount = budYield;
            pickableWeedPrefab.GetComponent<Pickable>().m_extraDrops.m_dropMin = budYieldBonus;
            pickableWeedPrefab.GetComponent<Pickable>().m_extraDrops.m_dropMax = budYieldBonus;
            pickableWeedPrefab.GetComponent<Pickable>().m_extraDrops.m_dropChance = budYieldBonusChance;

            weedSeedSaplingPrefab.GetComponent<Plant>().m_growTime = seedGrowTime;
            weedSeedSaplingPrefab.GetComponent<Plant>().m_growTimeMax = seedGrowTime;
            weedSeedSaplingPrefab.GetComponent<Plant>().m_biome = (Heightmap.Biome)25;
            pickableSeedWeedPrefab.GetComponent<Pickable>().m_amount = seedYield;
            pickableSeedWeedPrefab.GetComponent<Pickable>().m_extraDrops.m_dropMin = seedYieldBonus;
            pickableSeedWeedPrefab.GetComponent<Pickable>().m_extraDrops.m_dropMax = seedYieldBonus;
            pickableSeedWeedPrefab.GetComponent<Pickable>().m_extraDrops.m_dropChance = seedYieldBonusChance;
            
            PrefabManager.Instance.AddPrefab(pickableWeedPrefab);
            PrefabManager.Instance.AddPrefab(pickableSeedWeedPrefab);
            PrefabManager.Instance.AddPrefab(weedNugPrefab);

            PrefabManager.Instance.AddPrefab(bongSmokePrefab);
            PrefabManager.Instance.AddPrefab(bongNoisePrefab);
            PrefabManager.Instance.AddPrefab(mouthSmokePrefab);
            PrefabManager.Instance.AddPrefab(jointNoisePrefab);
            PrefabManager.Instance.AddPrefab(bongMouthSmokePrefab);
        }

        private void AddStatusEffects()
        {
            
            spriteBundle = AssetUtils.LoadAssetBundleFromResources("high_eyes", Assembly.GetExecutingAssembly());
            Sprite statusIcon = spriteBundle.LoadAsset<Sprite>("Assets/Joint/high_eyes.png");

            // Smoking effect from mouth (Joints)
            EffectList.EffectData[] mouthEffects = new EffectList.EffectData[2];
            EffectList.EffectData mouthEffect = new EffectList.EffectData
            {
                m_prefab = mouthSmokePrefab,
                m_attach = true,
                m_enabled = true
            };
            EffectList.EffectData mouthNoise = new EffectList.EffectData
            {
                m_prefab = jointNoisePrefab,
                m_enabled = true
            };
            mouthEffects[0] = mouthEffect;
            mouthEffects[1] = mouthNoise;

            // Smoking effect from mouth (Bong)
            EffectList.EffectData[] bongMouthEffects = new EffectList.EffectData[2];
            EffectList.EffectData bongMouthEffect = new EffectList.EffectData
            {
                m_prefab = bongMouthSmokePrefab,
                m_attach = true,
                m_enabled = true
            };
            EffectList.EffectData bongMouthSound = new EffectList.EffectData
            {
                m_prefab = bongInhaleNoisePrefab,
                m_attach = true,
                m_enabled = true
            };
            bongMouthEffects[0] = bongMouthEffect;
            bongMouthEffects[1] = bongMouthSound;

            if (statusIcon == null)
                Jotunn.Logger.LogInfo("statusIcon NULL");

            string replaceString;

            SE_Hybrid hybridEffect = ScriptableObject.CreateInstance<SE_Hybrid>();
            // Add config values
            hybridEffect.healthRegenVal = jointHealthRegenVal;
            hybridEffect.staminaRegenVal = jointStamRegenVal;
            hybridEffect.ttl = jointEffectTime;
            hybridEffect.cosmeticOnly = cosmeticOnly;

            hybridEffect.name = "HybridJointStatusEffect";
            hybridEffect.m_name = "$hybrid_joint_effectname";
            hybridEffect.m_icon = statusIcon;
            hybridEffect.m_startMessageType = MessageHud.MessageType.Center;
            hybridEffect.m_startMessage = "$joint_effectstart";
            hybridEffect.m_stopMessageType = MessageHud.MessageType.Center;
            hybridEffect.m_stopMessage = "$joint_effectstop";
            replaceString = "$hybrid_joint_tooltip";
            string hybridJointTooltip = $"{replaceString}";
            hybridEffect.m_tooltip = hybridJointTooltip;
            hybridEffect.m_startEffects.m_effectPrefabs = mouthEffects;
            hybridJointEffect = new CustomStatusEffect(hybridEffect, fixReference: false); 
            ItemManager.Instance.AddStatusEffect(hybridJointEffect);

            SE_Indica indicaEffect = ScriptableObject.CreateInstance<SE_Indica>();
            // Add config values
            indicaEffect.healthRegenVal = jointHealthRegenVal;
            indicaEffect.staminaRegenVal = jointStamRegenVal;
            indicaEffect.ttl = jointEffectTime;
            indicaEffect.cosmeticOnly = cosmeticOnly;

            indicaEffect.name = "IndicaJointStatusEffect";
            indicaEffect.m_name = "$indica_joint_effectname";
            indicaEffect.m_icon = statusIcon;
            indicaEffect.m_startMessageType = MessageHud.MessageType.Center;
            indicaEffect.m_startMessage = "$joint_effectstart";
            indicaEffect.m_stopMessageType = MessageHud.MessageType.Center;
            indicaEffect.m_stopMessage = "$joint_effectstop";
            replaceString = "$indica_joint_tooltip";
            string indicaJointTooltip = $"{replaceString}";
            indicaEffect.m_tooltip = indicaJointTooltip;
            indicaEffect.m_startEffects.m_effectPrefabs = mouthEffects;
            indicaJointEffect = new CustomStatusEffect(indicaEffect, fixReference: false);
            ItemManager.Instance.AddStatusEffect(indicaJointEffect);


            SE_Sativa sativaEffect = ScriptableObject.CreateInstance<SE_Sativa>();
            // Add config values
            sativaEffect.healthRegenVal = jointHealthRegenVal;
            sativaEffect.staminaRegenVal = jointStamRegenVal;
            sativaEffect.ttl = jointEffectTime;
            sativaEffect.cosmeticOnly = cosmeticOnly;

            sativaEffect.name = "SativaJointStatusEffect";
            sativaEffect.m_name = "$sativa_joint_effectname";
            sativaEffect.m_icon = statusIcon;
            sativaEffect.m_startMessageType = MessageHud.MessageType.Center;
            sativaEffect.m_startMessage = "$joint_effectstart";
            sativaEffect.m_stopMessageType = MessageHud.MessageType.Center;
            sativaEffect.m_stopMessage = "$joint_effectstop";
            replaceString = "$sativa_joint_tooltip";
            string sativaJointTooltip = $"{replaceString}{(cosmeticOnly ? "" : "\n\nHunger Rate -50%")}";
            sativaEffect.m_tooltip = sativaJointTooltip;
            sativaEffect.m_startEffects.m_effectPrefabs = mouthEffects;
            sativaJointEffect = new CustomStatusEffect(sativaEffect, fixReference: false);
            ItemManager.Instance.AddStatusEffect(sativaJointEffect);

            SE_Bong bongEffect = ScriptableObject.CreateInstance<SE_Bong>();

            // Add config values
            bongEffect.healthRegenVal = jointHealthRegenVal;
            bongEffect.staminaRegenVal = jointStamRegenVal;
            bongEffect.ttl = jointEffectTime * 3;
            bongEffect.cosmeticOnly = cosmeticOnly;

            bongEffect.name = "BongStatusEffect";
            bongEffect.m_name = "$bong_effectname";
            bongEffect.m_icon = statusIcon;
            bongEffect.m_startMessageType = MessageHud.MessageType.Center;
            bongEffect.m_startMessage = "$bong_effectstart";
            bongEffect.m_stopMessageType = MessageHud.MessageType.Center;
            bongEffect.m_stopMessage = "$joint_effectstop";
            replaceString = "$bong_tooltip";
            string bongTooltip = $"{replaceString}{(cosmeticOnly ? "" : "\nHunger Rate -50%")}";
            bongEffect.m_tooltip = bongTooltip;
            bongEffect.m_startEffects.m_effectPrefabs = bongMouthEffects;
            bongStatusEffect = new CustomStatusEffect(bongEffect, fixReference: false);
            ItemManager.Instance.AddStatusEffect(bongStatusEffect);
        }

        private void CreateItems()
        {
            CustomItem hybridJoint = new CustomItem(hybridJointPrefab, fixReference: false,
                new ItemConfig
                {
                    Amount = 1,
                    Requirements = new[]
                    {
                        new RequirementConfig { Item = "JointPaper", Amount = 1 },
                        new RequirementConfig { Item = "WeedBuds", Amount = 1 },
                        new RequirementConfig { Item = "Raspberry", Amount = 1 }
                    }
                });
            hybridJoint.ItemDrop.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Consumable;
            hybridJoint.ItemDrop.m_itemData.m_shared.m_consumeStatusEffect = hybridJointEffect.StatusEffect;
            ItemManager.Instance.AddItem(hybridJoint);

            CustomItem indicaJoint = new CustomItem(indicaJointPrefab, fixReference: false,
                new ItemConfig
                {
                    Amount = 1,
                    Requirements = new[]
                    {
                        new RequirementConfig { Item = "JointPaper", Amount = 1 },
                        new RequirementConfig { Item = "WeedBuds", Amount = 1 },
                        new RequirementConfig { Item = "Blueberries", Amount = 1 }
                    }
                });
            indicaJoint.ItemDrop.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Consumable;
            indicaJoint.ItemDrop.m_itemData.m_shared.m_consumeStatusEffect = indicaJointEffect.StatusEffect;
            ItemManager.Instance.AddItem(indicaJoint);

            CustomItem sativaJoint = new CustomItem(sativaJointPrefab, fixReference: false,
                new ItemConfig
                {
                    Amount = 1,
                    Requirements = new[]
                    {
                        new RequirementConfig { Item = "JointPaper", Amount = 1 },
                        new RequirementConfig { Item = "WeedBuds", Amount = 1 },
                        new RequirementConfig { Item = "Honey", Amount = 1 }
                    }
                });
            sativaJoint.ItemDrop.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Consumable;
            sativaJoint.ItemDrop.m_itemData.m_shared.m_consumeStatusEffect = sativaJointEffect.StatusEffect;
            ItemManager.Instance.AddItem(sativaJoint);

            CustomItem weed_papers = new CustomItem(weedPaperPrefab, fixReference: false,
                new ItemConfig
                {
                    Amount = 1,
                    Requirements = new[]
                    {
                      new RequirementConfig { Item = "Dandelion", Amount = 2 }
                    }
                });
            ItemManager.Instance.AddItem(weed_papers);


            CustomItem weed_nugs = new CustomItem(weedNugsPrefab, fixReference: false);
            ItemManager.Instance.AddItem(weed_nugs);

            CustomItem weed_seeds = new CustomItem(weedSeedsPrefab, fixReference: false,
                new ItemConfig
                {
                    Amount = 1,
                    Requirements = new[]
                    {
                        new RequirementConfig { Item = "AncientSeed", Amount = 3 }
                    }
                });
            ItemManager.Instance.AddItem(weed_seeds);
        }

        private void CreatePieces()
        {
            CustomPiece weedPlant = new CustomPiece(weedSaplingPrefab, true,
                new PieceConfig
                {
                    PieceTable = "Cultivator",
                    Requirements = new[]
                    {
                        new RequirementConfig { Item = "WeedSeeds", Amount = 1 }
                    }
                });
            
            PieceManager.Instance.AddPiece(weedPlant);

            CustomPiece weedSeedPlant = new CustomPiece(weedSeedSaplingPrefab, true,
                new PieceConfig
                {
                    PieceTable = "Cultivator",
                    Requirements = new[]
                    {
                        new RequirementConfig { Item = "WeedBuds", Amount = 1 }
                    }
                });
            
            PieceManager.Instance.AddPiece(weedSeedPlant);

            CustomPiece bong = new CustomPiece(bongPrefab, true,
                new PieceConfig
                {
                    PieceTable = "Hammer",
                    Requirements = new[]
                    {
                        new RequirementConfig { Item = "BronzeNails", Amount = 5 },
                        new RequirementConfig { Item = "Resin", Amount = 10 },
                        new RequirementConfig { Item = "TrophySkeleton", Amount = 1 },
                        new RequirementConfig { Item = "GreydwarfEye", Amount = 5 }
                    }
                });
            bong.Piece.GetComponent<Piece>().m_resources[0].m_recover = true;
            bong.Piece.GetComponent<Piece>().m_resources[1].m_recover = true;
            bong.Piece.GetComponent<Piece>().m_resources[2].m_recover = true;
            bong.Piece.GetComponent<Piece>().m_resources[3].m_recover = true;

            // Bong effect 
            bong.PiecePrefab.AddComponent<Bong>();
            bong.PiecePrefab.GetComponent<Bong>().m_name = "$piece_bong";
            bong.PiecePrefab.GetComponent<Bong>().m_startFuel = 0;
            bong.PiecePrefab.GetComponent<Bong>().m_maxFuel = 1;
            bong.PiecePrefab.GetComponent<Bong>().m_secPerFuel = 999999999;
            bong.PiecePrefab.GetComponent<Bong>().m_checkTerrainOffset = 0;
            bong.PiecePrefab.GetComponent<Bong>().m_coverCheckOffset = 0;
            bong.PiecePrefab.GetComponent<Bong>().m_fuelItem = weedNugsPrefab.GetComponent<ItemDrop>();
            bong.PiecePrefab.GetComponent<Bong>().cosmeticOnly = cosmeticOnly;


            // Bong noise/smoke update
            bongSmokePrefab.transform.localScale = new Vector3((float)0.1, (float)0.1, (float)0.1);

            EffectList.EffectData[] bongEffects = new EffectList.EffectData[2];
            EffectList.EffectData bongEffect = new EffectList.EffectData
            {
                m_prefab = bongNoisePrefab,
                m_enabled = true
            };
            EffectList.EffectData bongSmoke = new EffectList.EffectData
            {
                m_prefab = bongSmokePrefab,
                m_enabled = true,
                m_inheritParentScale = true,
                m_attach = true
            };
            bongEffects[0] = bongEffect;
            bongEffects[1] = bongSmoke;

            bong.PiecePrefab.GetComponent<Bong>().m_fuelAddedEffects.m_effectPrefabs = bongEffects;
            bong.Piece.GetComponent<Bong>().ttl = jointEffectTime / 2;
            bong.PiecePrefab.GetComponent<Piece>().m_onlyInBiome = (Heightmap.Biome)0;
      
            PieceManager.Instance.AddPiece(bong);
        }

        // Make plants growable indoors
        [HarmonyPatch(typeof(Plant), "HaveRoof")]
        public static class PlantHaveRoof
        {
            public static bool Prefix(Plant __instance, ref bool __result)
            {
                if (__instance.m_name == "$piece_sapling_weed" || __instance.m_name == "$piece_sapling_weed_seeds")
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Player), "CanConsumeItem")]
        public static class PlayerCanConsumeItem
        {
            public static bool Prefix(Player __instance, ref ItemDrop.ItemData item, ref bool __result)
            {
                if ((bool)item.m_shared.m_consumeStatusEffect)
                {
                    StatusEffect consumeStatusEffect = item.m_shared.m_consumeStatusEffect;
                    if (item.m_shared.m_consumeStatusEffect.m_name.EndsWith("joint_effectname") && __instance.m_seman.HaveStatusEffect(item.m_shared.m_consumeStatusEffect.name) || __instance.m_seman.HaveStatusEffectCategory(consumeStatusEffect.m_category))
                    {
                        __result = true;
                        return false;
                    }
                }
                return true;
            }    
        }
    }
}