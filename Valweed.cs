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

namespace Valweed
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class Valweed : BaseUnityPlugin
    {
        public const string PluginGUID = "com.drod917.Valweed";
        public const string PluginName = "Valweed";
        public const string PluginVersion = "0.2.8";

        // 0.2.8
        // Update Jotunn dependency to 2.3.0

        // 0.2.7
        // Removed unnecessary code that caused errors in the coming Jotunn update.

        // 0.2.6
        // Fixed crashing on quit - Unloaded bundle issue

        // 0.2.5 
        // Fixed the console spam issue
        // TODO: Console still gets spammed when joint status effect wears off and you move far fast

        private AssetBundle jointResourceBundle;
        private AssetBundle plantResourceBundle;
        private AssetBundle bongResourceBundle;
        AssetBundle spriteBundle;

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

        private float jointHealthRegenVal;
        private float jointStamRegenVal;
        private float jointEffectTime;
        private float jointHealthRegenMult;
        private float jointStamRegenMult;

        private void Awake()
        {
            jointHealthRegenVal = Config.Bind<int>("Main Section", "Percent rate to increase health regen (Default 50 (+50%))", 50,
                new ConfigDescription("Restart the game for this change to take effect.", new AcceptableValueRange<int>(0, 1000))).Value;

            jointStamRegenVal = Config.Bind<int>("Main Section", "Percent rate to increase stamina regen (Default 100 (+100%))", 100, 
                new ConfigDescription("Restart the game for this change to take effect.", new AcceptableValueRange<int>(0, 1000))).Value;

            jointEffectTime = Config.Bind<int>("Main Section", "Effective time for smoking to last (Default 600s (10m))", 600,
                new ConfigDescription("Restart the game for this change to take effect.", new AcceptableValueRange<int>(20, 3600))).Value;

            jointHealthRegenMult = 1 + jointHealthRegenVal / 100;
            jointStamRegenMult = 1 + jointStamRegenVal / 100;

            LoadAssets();
            AddStatusEffects();
            CreateItems();
            CreatePieces();
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
            hybridJointPrefab = jointResourceBundle.LoadAsset<GameObject>("joint_hybrid");
            indicaJointPrefab = jointResourceBundle.LoadAsset<GameObject>("joint_indica");
            sativaJointPrefab = jointResourceBundle.LoadAsset<GameObject>("joint_sativa");
            weedPaperPrefab = jointResourceBundle.LoadAsset<GameObject>("joint_paper");
            jointNoisePrefab = jointResourceBundle.LoadAsset<GameObject>("sfx_hit_joint");

            // Growable stuff
            plantResourceBundle = AssetUtils.LoadAssetBundleFromResources("plant", Assembly.GetExecutingAssembly());
            weedSaplingPrefab = plantResourceBundle.LoadAsset<GameObject>("sapling_weed");
            weedSeedSaplingPrefab = plantResourceBundle.LoadAsset<GameObject>("sapling_seedweed");
            pickableWeedPrefab = plantResourceBundle.LoadAsset<GameObject>("Pickable_WeedPlant");
            pickableSeedWeedPrefab = plantResourceBundle.LoadAsset<GameObject>("Pickable_SeedWeedPlant");
            // ItemDrop stuff
            weedNugsPrefab = plantResourceBundle.LoadAsset<GameObject>("WeedBuds");
            weedSeedsPrefab = plantResourceBundle.LoadAsset<GameObject>("weed_seeds");

            bongResourceBundle = AssetUtils.LoadAssetBundleFromResources("bong", Assembly.GetExecutingAssembly());
            bongPrefab = bongResourceBundle.LoadAsset<GameObject>("Bong");
            weedNugPrefab = bongResourceBundle.LoadAsset<GameObject>("bud");
            bongNoisePrefab = bongResourceBundle.LoadAsset<GameObject>("sfx_hit_bong");
            bongInhaleNoisePrefab = bongResourceBundle.LoadAsset<GameObject>("sfx_hit_bong_inhale");
            bongSmokePrefab = bongResourceBundle.LoadAsset<GameObject>("vfx_bong_smoke");
            mouthSmokePrefab = bongResourceBundle.LoadAsset<GameObject>("vfx_mouth_smoke");
            bongMouthSmokePrefab = bongResourceBundle.LoadAsset<GameObject>("vfx_mouth_smoke_bong");
            
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

            SE_Hybrid hybridEffect = ScriptableObject.CreateInstance<SE_Hybrid>();
            // Add config values
            hybridEffect.healthRegenMult = jointHealthRegenMult;
            hybridEffect.staminaRegenMult = jointStamRegenMult;
            hybridEffect.ttl = jointEffectTime;

            hybridEffect.name = "HybridJointStatusEffect";
            hybridEffect.m_name = "$hybrid_joint_effectname";
            hybridEffect.m_icon = statusIcon;
            hybridEffect.m_startMessageType = MessageHud.MessageType.Center;
            hybridEffect.m_startMessage = "$joint_effectstart";
            hybridEffect.m_stopMessageType = MessageHud.MessageType.Center;
            hybridEffect.m_stopMessage = "$joint_effectstop";
            hybridEffect.m_tooltip = $"You feel balanced.";
            hybridEffect.m_startEffects.m_effectPrefabs = mouthEffects;
            hybridJointEffect = new CustomStatusEffect(hybridEffect, fixReference: false); 
            ItemManager.Instance.AddStatusEffect(hybridJointEffect);

            SE_Indica indicaEffect = ScriptableObject.CreateInstance<SE_Indica>();
            // Add config values
            indicaEffect.healthRegenMult = jointHealthRegenMult;
            indicaEffect.staminaRegenMult = jointStamRegenMult;
            indicaEffect.ttl = jointEffectTime;

            indicaEffect.name = "IndicaJointStatusEffect";
            indicaEffect.m_name = "$indica_joint_effectname";
            indicaEffect.m_icon = statusIcon;
            indicaEffect.m_startMessageType = MessageHud.MessageType.Center;
            indicaEffect.m_startMessage = "$joint_effectstart";
            indicaEffect.m_stopMessageType = MessageHud.MessageType.Center;
            indicaEffect.m_stopMessage = "$joint_effectstop";
            indicaEffect.m_tooltip = $"You feel relaxed.\nMakes you Rested.\nIf you are already Rested, smoking will add {jointEffectTime / 60}m to the effect.";
            indicaEffect.m_startEffects.m_effectPrefabs = mouthEffects;
            indicaJointEffect = new CustomStatusEffect(indicaEffect, fixReference: false);
            ItemManager.Instance.AddStatusEffect(indicaJointEffect);


            SE_Sativa sativaEffect = ScriptableObject.CreateInstance<SE_Sativa>();
            // Add config values
            sativaEffect.healthRegenMult = jointHealthRegenMult;
            sativaEffect.staminaRegenMult = jointStamRegenMult;
            sativaEffect.ttl = jointEffectTime;

            sativaEffect.name = "SativaJointStatusEffect";
            sativaEffect.m_name = "$sativa_joint_effectname";
            sativaEffect.m_icon = statusIcon;
            sativaEffect.m_startMessageType = MessageHud.MessageType.Center;
            sativaEffect.m_startMessage = "$joint_effectstart";
            sativaEffect.m_stopMessageType = MessageHud.MessageType.Center;
            sativaEffect.m_stopMessage = "$joint_effectstop";
            sativaEffect.m_tooltip = $"You feel motivated.\nHunger rate -50%";
            sativaEffect.m_startEffects.m_effectPrefabs = mouthEffects;
            sativaJointEffect = new CustomStatusEffect(sativaEffect, fixReference: false);
            ItemManager.Instance.AddStatusEffect(sativaJointEffect);

            SE_Bong bongEffect = ScriptableObject.CreateInstance<SE_Bong>();
            // Add config values
            bongEffect.healthRegenMult = jointHealthRegenMult;
            bongEffect.staminaRegenMult = jointStamRegenMult;
            bongEffect.ttl = jointEffectTime * 3;

            bongEffect.name = "BongStatusEffect";
            bongEffect.m_name = "$bong_effectname";
            bongEffect.m_icon = statusIcon;
            bongEffect.m_startMessageType = MessageHud.MessageType.Center;
            bongEffect.m_startMessage = "$bong_effectstart";
            bongEffect.m_stopMessageType = MessageHud.MessageType.Center;
            bongEffect.m_stopMessage = "$joint_effectstop";
            bongEffect.m_tooltip = $"You feel ZOOTED.\nAll three joint effects are combined.\nHunger rate -50%\nMakes you Rested.";
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
                        new RequirementConfig { Item = "joint_paper", Amount = 1 },
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
                        new RequirementConfig { Item = "joint_paper", Amount = 1 },
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
                        new RequirementConfig { Item = "joint_paper", Amount = 1 },
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
                        new RequirementConfig { Item = "weed_seeds", Amount = 1 }
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
    }
}