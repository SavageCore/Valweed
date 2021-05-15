﻿// File:    Valweed.cs
// Project: Valweed
using BepInEx;
using UnityEngine;
using BepInEx.Configuration;
using Jotunn.Utils;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Configs;
using System.Reflection;

namespace Valweed
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class Valweed : BaseUnityPlugin
    {
        public const string PluginGUID = "com.drod917.Valweed";
        public const string PluginName = "Valweed";
        public const string PluginVersion = "0.0.5.1";

        // 0051 updates
        // Indica joint typo fix
        // Fix plant biome placement fix (only used to work in Meadows)
        // TODO: Place bong

        private AssetBundle jointResourceBundle;
        private AssetBundle plantResourceBundle;
        private AssetBundle bongResourceBundle;

        private GameObject hybridJointPrefab;
        private GameObject indicaJointPrefab;
        private GameObject sativaJointPrefab;
        private GameObject weedSaplingPrefab;
        private GameObject weedSeedSaplingPrefab;
        private GameObject weedNugsPrefab;
        private GameObject weedSeedsPrefab;
        private GameObject weedPaperPrefab;
        private GameObject bongPrefab;

        private CustomStatusEffect hybridJointEffect;
        private CustomStatusEffect indicaJointEffect;
        private CustomStatusEffect sativaJointEffect;

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
                new ConfigDescription("Restart the game for this change to take effect.", new AcceptableValueRange<int>(1, 3600))).Value;

            jointHealthRegenMult = 1 + jointHealthRegenVal / 100;
            jointStamRegenMult = 1 + jointStamRegenVal / 100;

            LoadAssets();
            AddLocalizations();
            AddStatusEffects();
            CreateBlueprintRune();
            CreateRunePieces();
        }

        private void Update()
        {
        }

        private void LoadAssets()
        {
            //Load embedded resources
            //Jotunn.Logger.LogInfo($"Embedded resources: {string.Join(",", Assembly.GetExecutingAssembly().GetManifestResourceNames())}");
            jointResourceBundle = AssetUtils.LoadAssetBundleFromResources("joint", Assembly.GetExecutingAssembly());
            hybridJointPrefab = jointResourceBundle.LoadAsset<GameObject>("Assets/Joint/Joint_hybrid.prefab");
            indicaJointPrefab = jointResourceBundle.LoadAsset<GameObject>("Assets/Joint/Joint_indica.prefab");
            sativaJointPrefab = jointResourceBundle.LoadAsset<GameObject>("Assets/Joint/Joint_sativa.prefab");
            weedPaperPrefab = jointResourceBundle.LoadAsset<GameObject>("Assets/Joint/Joint_paper.prefab");

            plantResourceBundle = AssetUtils.LoadAssetBundleFromResources("plant", Assembly.GetExecutingAssembly());
            weedSaplingPrefab = plantResourceBundle.LoadAsset<GameObject>("Assets/Plant/Seed/sapling_weed.prefab");
            weedSeedSaplingPrefab = plantResourceBundle.LoadAsset<GameObject>("Assets/Plant/Seed/sapling_weed_seeds.prefab");
            weedNugsPrefab = plantResourceBundle.LoadAsset<GameObject>("Assets/Plant/Seed/weed_buds.prefab");
            weedSeedsPrefab = plantResourceBundle.LoadAsset<GameObject>("Assets/Plant/Seed/weed_seeds.prefab");

            //bongResourceBundle = AssetUtils.LoadAssetBundleFromResources("bong", Assembly.GetExecutingAssembly());
            //bongPrefab = bongResourceBundle.LoadAsset<GameObject>("Assets/Bong/bong.prefab");
        }

        // Adds localizations with configs
        private void AddLocalizations()
        {
            LocalizationManager.Instance.AddLocalization(new LocalizationConfig("English")
            {
                Translations = {
                    {"piece_bong_add", "Add to bong"},
                    {"piece_bong_tap", "Smoke"},
                    {"piece_bong", "Bong"},
                    {"piece_bong_desc", "For when you need to soar."},
                    {"item_weed_seeds", "Weed Seeds"},
                    {"item_weed_seeds_desc", "Seeds of the marijuana plant."},
                    {"item_weed_buds", "Weed Buds"},
                    {"item_weed_buds_desc", "Some very stinky bud."},
                    {"piece_sapling_weed_seeds", "Seed-Weed Plant"},
                    {"piece_sapling_weed_seeds_desc", "Plant this to grow some seedy weed."},
                    {"piece_sapling_weed", "Weed Plant"},
                    {"piece_sapling_weed_desc", "Plant this to grow some weed."},
                    {"item_joint_paper", "Joint Paper" },
                    {"item_joint_paper_desc", "Use these to roll up a joint."},
                    {"item_hybrid_joint", "Joint (Hybrid)"},
                    {"item_hybrid_joint_desc", "Smoke a fat doobie."},
                    {"item_indica_joint", "Joint (Indica)"},
                    {"item_indica_joint_desc", "Smoke a fat doobie."},
                    {"item_sativa_joint", "Joint (Sativa)"},
                    {"item_sativa_joint_desc", "Smoke a fat doobie."},
                    {"hybrid_joint_effectname", "High (Hybrid)"},
                    {"indica_joint_effectname", "High (Indica)"},
                    {"sativa_joint_effectname", "High (Sativa)"},
                    {"joint_effectstart", "You feel high."},
                    {"joint_effectstop", "You're coming down."}
                }
            });

            LocalizationManager.Instance.AddLocalization(new LocalizationConfig("German")
            {
                Translations =
                {
                    {"item_weed_seeds", "Weed Seeds"},
                    {"item_weed_seeds_desc", "Seeds of the marijuana plant."},
                    {"item_weed_buds", "Weed Buds"},
                    {"item_weed_buds_desc", "Some very stinky bud."},
                    {"piece_sapling_weed_seeds", "Seed-Weed Plant"},
                    {"piece_sapling_weed_seeds_desc", "Plant this to grow some seedy weed."},
                    {"piece_sapling_weed", "Weed Plant"},
                    {"piece_sapling_weed_desc", "Plant this to grow some weed."},
                    {"item_joint_paper", "Joint Paper" },
                    {"item_joint_paper_desc", "Use these to roll up a joint."}, //TODO
                    {"item_hybrid_joint", "Joint (Hybrid)"},
                    {"item_hybrid_joint_desc", "Rauchen Sie einen fetten Doobie."},
                    {"item_indica_joint", "Joint (Indica)"},
                    {"item_indica_joint_desc", "Rauchen Sie einen fetten Doobie."},
                    {"item_sativa_joint", "Joint (Sativa)"},
                    {"item_sativa_joint_desc", "Rauchen Sie einen fetten Doobie."},
                    {"hybrid_joint_effectname", "High (Hybrid)"},
                    {"indica_joint_effectname", "High (Indica)"},
                    {"sativa_joint_effectname", "High (Sativa)"},
                    {"joint_effectstart", "Du fühlst dich hoch."},
                    {"joint_effectstop", "Du kommst runter."}
                }
            });

            LocalizationManager.Instance.AddLocalization(new LocalizationConfig("Italian")
            {
                Translations =
                {
                    {"item_weed_seeds", "Weed Seeds"},
                    {"item_weed_seeds_desc", "Seeds of the marijuana plant."},
                    {"item_weed_buds", "Weed Buds"},
                    {"item_weed_buds_desc", "Some very stinky bud."},
                    {"piece_sapling_weed_seeds", "Seed-Weed Plant"},
                    {"piece_sapling_weed_seeds_desc", "Plant this to grow some seedy weed."},
                    {"piece_sapling_weed", "Weed Plant"},
                    {"piece_sapling_weed_desc", "Plant this to grow some weed."},
                    {"item_joint_paper", "Joint Paper" },
                    {"item_joint_paper_desc", "Use these to roll up a joint."}, //TODO
                    {"item_hybrid_joint", "Spinello (Hybrid)"},
                    {"item_hybrid_joint_desc", "Fuma un grasso doobie."},
                    {"item_indica_joint", "Spinello (Indica)"},
                    {"item_indica_joint_desc", "Fuma un grasso doobie."},
                    {"item_sativa_joint", "Spinello (Sativa)"},
                    {"item_sativa_joint_desc", "Fuma un grasso doobie."},
                    {"hybrid_joint_effectname", "High (Hybrid)"},
                    {"indica_joint_effectname", "High (Indica)"},
                    {"sativa_joint_effectname", "High (Sativa)"},
                    {"joint_effectstart", "Ti senti in alto."},
                    {"joint_effectstop", "Stai diventando sobrio."}
                }
            });

            LocalizationManager.Instance.AddLocalization(new LocalizationConfig("French")
            {
                Translations =
                {
                    {"item_weed_seeds", "Weed Seeds"},
                    {"item_weed_seeds_desc", "Seeds of the marijuana plant."},
                    {"item_weed_buds", "Weed Buds"},
                    {"item_weed_buds_desc", "Some very stinky bud."},
                    {"piece_sapling_weed_seeds", "Seed-Weed Plant"},
                    {"piece_sapling_weed_seeds_desc", "Plant this to grow some seedy weed."},
                    {"piece_sapling_weed", "Weed Plant"},
                    {"piece_sapling_weed_desc", "Plant this to grow some weed."},
                    {"item_joint_paper", "Joint Paper" },
                    {"item_joint_paper_desc", "Use these to roll up a joint."}, //TODO
                    {"item_hybrid_joint", "Pétard (Hybrid)"},
                    {"item_hybrid_joint_desc", "Fumer un gros doobie."},
                    {"item_indica_joint", "Pétard (Indica)"},
                    {"item_indica_joint_desc", "Fumer un gros doobie."},
                    {"item_sativa_joint", "Pétard (Sativa)"},
                    {"item_sativa_joint_desc", "Fumer un gros doobie."},
                    {"hybrid_joint_effectname", "High (Hybrid)"},
                    {"indica_joint_effectname", "High (Indica)"},
                    {"sativa_joint_effectname", "High (Sativa)"},
                    {"joint_effectstart", "Tu te sens haut."},
                    {"joint_effectstop", "Tu deviens sobre."}
                }
            });

            LocalizationManager.Instance.AddLocalization(new LocalizationConfig("Spanish")
            {
                Translations =
                {
                    {"item_weed_seeds", "Weed Seeds"},
                    {"item_weed_seeds_desc", "Seeds of the marijuana plant."},
                    {"item_weed_buds", "Weed Buds"},
                    {"item_weed_buds_desc", "Some very stinky bud."},
                    {"piece_sapling_weed_seeds", "Seed-Weed Plant"},
                    {"piece_sapling_weed_seeds_desc", "Plant this to grow some seedy weed."},
                    {"piece_sapling_weed", "Weed Plant"},
                    {"piece_sapling_weed_desc", "Plant this to grow some weed."},
                    {"item_joint_paper", "Joint Paper" },
                    {"item_joint_paper_desc", "Use these to roll up a joint."}, //TODO
                    {"item_hybrid_joint", "Porro (Hybrid)"},
                    {"item_hybrid_joint_desc", "Fuma un doobie gordo."},
                    {"item_indica_joint", "Porro (Indica)"},
                    {"item_indica_joint_desc", "Fuma un doobie gordo."},
                    {"item_sativa_joint", "Porro (Sativa)"},
                    {"item_sativa_joint_desc", "Fuma un doobie gordo."},
                    {"hybrid_joint_effectname", "High (Hybrid)"},
                    {"indica_joint_effectname", "High (Indica)"},
                    {"sativa_joint_effectname", "High (Sativa)"},
                    {"joint_effectstart", "Te sientes alto."},
                    {"joint_effectstop", "Te estás volviendo sobrio."}
                }
            });

            // TODO
            LocalizationManager.Instance.AddLocalization(new LocalizationConfig("Polish")
            {
                Translations = {
                    {"item_weed_seeds", "Weed Seeds"},
                    {"item_weed_seeds_desc", "Seeds of the marijuana plant."},
                    {"item_weed_buds", "Weed Buds"},
                    {"item_weed_buds_desc", "Some very stinky bud."},
                    {"piece_sapling_weed_seeds", "Seed-Weed Plant"},
                    {"piece_sapling_weed_seeds_desc", "Plant this to grow some seedy weed."},
                    {"piece_sapling_weed", "Weed Plant"},
                    {"piece_sapling_weed_desc", "Plant this to grow some weed."},
                    {"item_joint_paper", "Joint Paper" },
                    {"item_joint_paper_desc", "Use these to roll up a joint."}, //TODO
                    {"item_hybrid_joint", "Joint (Hybrid)"},
                    {"item_hybrid_joint_desc", "Smoke a fat doobie."},
                    {"item_indica_joint", "Joint (Indica)"},
                    {"item_indica_joint_desc", "Smoke a fat doobie."},
                    {"item_sativa_joint", "Joint (Sativa)"},
                    {"item_sativa_joint_desc", "Smoke a fat doobie."},
                    {"hybrid_joint_effectname", "High (Hybrid)"},
                    {"indica_joint_effectname", "High (Indica)"},
                    {"sativa_joint_effectname", "High (Sativa)"},
                    {"joint_effectstart", "You feel high."},
                    {"joint_effectstop", "You're coming down."}
                }
            });

            LocalizationManager.Instance.AddLocalization(new LocalizationConfig("Russian")
            {
                Translations = {
                    {"item_weed_seeds", "Weed Seeds"},
                    {"item_weed_seeds_desc", "Seeds of the marijuana plant."},
                    {"item_weed_buds", "Weed Buds"},
                    {"item_weed_buds_desc", "Some very stinky bud."},
                    {"piece_sapling_weed_seeds", "Seed-Weed Plant"},
                    {"piece_sapling_weed_seeds_desc", "Plant this to grow some seedy weed."},
                    {"piece_sapling_weed", "Weed Plant"},
                    {"piece_sapling_weed_desc", "Plant this to grow some weed."},
                    {"item_joint_paper", "Joint Paper" },
                    {"item_joint_paper_desc", "Use these to roll up a joint."}, //TODO
                    {"item_hybrid_joint", "Joint (Hybrid)"},
                    {"item_hybrid_joint_desc", "Smoke a fat doobie."},
                    {"item_indica_joint", "Joint (Indica)"},
                    {"item_indica_joint_desc", "Smoke a fat doobie."},
                    {"item_sativa_joint", "Joint (Sativa)"},
                    {"item_sativa_joint_desc", "Smoke a fat doobie."},
                    {"hybrid_joint_effectname", "High (Hybrid)"},
                    {"indica_joint_effectname", "High (Indica)"},
                    {"sativa_joint_effectname", "High (Sativa)"},
                    {"joint_effectstart", "You feel high."},
                    {"joint_effectstop", "You're coming down."}
                }
            });

            LocalizationManager.Instance.AddLocalization(new LocalizationConfig("Turkish")
            {
                Translations = {
                    {"item_weed_seeds", "Weed Seeds"},
                    {"item_weed_seeds_desc", "Seeds of the marijuana plant."},
                    {"item_weed_buds", "Weed Buds"},
                    {"item_weed_buds_desc", "Some very stinky bud."},
                    {"piece_sapling_weed_seeds", "Seed-Weed Plant"},
                    {"piece_sapling_weed_seeds_desc", "Plant this to grow some seedy weed."},
                    {"piece_sapling_weed", "Weed Plant"},
                    {"piece_sapling_weed_desc", "Plant this to grow some weed."},
                    {"item_joint_paper", "Joint Paper" },
                    {"item_joint_paper_desc", "Use these to roll up a joint."}, //TODO
                    {"item_hybrid_joint", "Joint (Hybrid)"},
                    {"item_hybrid_joint_desc", "Smoke a fat doobie."},
                    {"item_indica_joint", "Joint (Indica)"},
                    {"item_indica_joint_desc", "Smoke a fat doobie."},
                    {"item_sativa_joint", "Joint (Sativa)"},
                    {"item_sativa_joint_desc", "Smoke a fat doobie."},
                    {"hybrid_joint_effectname", "High (Hybrid)"},
                    {"indica_joint_effectname", "High (Indica)"},
                    {"sativa_joint_effectname", "High (Sativa)"},
                    {"joint_effectstart", "You feel high."},
                    {"joint_effectstop", "You're coming down."}
                }
            });

            LocalizationManager.Instance.AddLocalization(new LocalizationConfig("Dutch")
            {
                Translations = {
                    {"item_weed_seeds", "Weed Seeds"},
                    {"item_weed_seeds_desc", "Seeds of the marijuana plant."},
                    {"item_weed_buds", "Weed Buds"},
                    {"item_weed_buds_desc", "Some very stinky bud."},
                    {"piece_sapling_weed_seeds", "Seed-Weed Plant"},
                    {"piece_sapling_weed_seeds_desc", "Plant this to grow some seedy weed."},
                    {"piece_sapling_weed", "Weed Plant"},
                    {"piece_sapling_weed_desc", "Plant this to grow some weed."},
                    {"item_joint_paper", "Joint Paper" },
                    {"item_joint_paper_desc", "Use these to roll up a joint."}, //TODO
                    {"item_hybrid_joint", "Joint (Hybrid)"},
                    {"item_hybrid_joint_desc", "Smoke a fat doobie."},
                    {"item_indica_joint", "Joint (Indica)"},
                    {"item_indica_joint_desc", "Smoke a fat doobie."},
                    {"item_sativa_joint", "Joint (Sativa)"},
                    {"item_sativa_joint_desc", "Smoke a fat doobie."},
                    {"hybrid_joint_effectname", "High (Hybrid)"},
                    {"indica_joint_effectname", "High (Indica)"},
                    {"sativa_joint_effectname", "High (Sativa)"},
                    {"joint_effectstart", "You feel high."},
                    {"joint_effectstop", "You're coming down."}
                }
            });

            LocalizationManager.Instance.AddLocalization(new LocalizationConfig("Simplified Chinese")
            {
                Translations = {
                    {"item_weed_seeds", "Weed Seeds"},
                    {"item_weed_seeds_desc", "Seeds of the marijuana plant."},
                    {"item_weed_buds", "Weed Buds"},
                    {"item_weed_buds_desc", "Some very stinky bud."},
                    {"piece_sapling_weed_seeds", "Seed-Weed Plant"},
                    {"piece_sapling_weed_seeds_desc", "Plant this to grow some seedy weed."},
                    {"piece_sapling_weed", "Weed Plant"},
                    {"piece_sapling_weed_desc", "Plant this to grow some weed."},
                    {"item_joint_paper", "Joint Paper" },
                    {"item_joint_paper_desc", "Use these to roll up a joint."}, //TODO
                    {"item_hybrid_joint", "Joint (Hybrid)"},
                    {"item_hybrid_joint_desc", "Smoke a fat doobie."},
                    {"item_indica_joint", "Joint (Indica)"},
                    {"item_indica_joint_desc", "Smoke a fat doobie."},
                    {"item_sativa_joint", "Joint (Sativa)"},
                    {"item_sativa_joint_desc", "Smoke a fat doobie."},
                    {"hybrid_joint_effectname", "High (Hybrid)"},
                    {"indica_joint_effectname", "High (Indica)"},
                    {"sativa_joint_effectname", "High (Sativa)"},
                    {"joint_effectstart", "You feel high."},
                    {"joint_effectstop", "You're coming down."}
                }
            });

            LocalizationManager.Instance.AddLocalization(new LocalizationConfig("Japanese")
            {
                Translations = {
                    {"item_weed_seeds", "Weed Seeds"},
                    {"item_weed_seeds_desc", "Seeds of the marijuana plant."},
                    {"item_weed_buds", "Weed Buds"},
                    {"item_weed_buds_desc", "Some very stinky bud."},
                    {"piece_sapling_weed_seeds", "Seed-Weed Plant"},
                    {"piece_sapling_weed_seeds_desc", "Plant this to grow some seedy weed."},
                    {"piece_sapling_weed", "Weed Plant"},
                    {"piece_sapling_weed_desc", "Plant this to grow some weed."},
                    {"item_joint_paper", "Joint Paper" },
                    {"item_joint_paper_desc", "Use these to roll up a joint."}, //TODO
                    {"item_hybrid_joint", "Joint (Hybrid)"},
                    {"item_hybrid_joint_desc", "Smoke a fat doobie."},
                    {"item_indica_joint", "Joint (Indica)"},
                    {"item_indica_joint_desc", "Smoke a fat doobie."},
                    {"item_sativa_joint", "Joint (Sativa)"},
                    {"item_sativa_joint_desc", "Smoke a fat doobie."},
                    {"hybrid_joint_effectname", "High (Hybrid)"},
                    {"indica_joint_effectname", "High (Indica)"},
                    {"sativa_joint_effectname", "High (Sativa)"},
                    {"joint_effectstart", "You feel high."},
                    {"joint_effectstop", "You're coming down."}
                }
            });
        }

        private void AddStatusEffects()
        {
            AssetBundle spriteBundle = AssetUtils.LoadAssetBundleFromResources("high_eyes", Assembly.GetExecutingAssembly());
            Sprite statusIcon = spriteBundle.LoadAsset<Sprite>("Assets/Joint/high_eyes.png");

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
            hybridEffect.m_tooltip = $"You feel balanced.\nHealth regen +{jointHealthRegenVal}%\nStamina regen +{jointStamRegenVal}%";
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
            indicaEffect.m_tooltip = $"You feel relaxed.\nMakes you Rested.\nIf you are already Rested, smoking will add {jointEffectTime / 60}m to the effect." +
                                        $"\nHealth regen +{jointHealthRegenVal}%\nStamina regen +{jointStamRegenVal}%";
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
            sativaEffect.m_tooltip = $"You feel motivated.\nHealth regen +{jointHealthRegenVal}%\nStamina regen +{jointStamRegenVal}%\nHunger rate -50%";
            sativaJointEffect = new CustomStatusEffect(sativaEffect, fixReference: false);
            ItemManager.Instance.AddStatusEffect(sativaJointEffect);
        }

        private void CreateBlueprintRune()
        {
            CustomItem hj_rune = new CustomItem(hybridJointPrefab, fixReference: false,
                new ItemConfig
                {
                    Amount = 1,
                    Requirements = new[]
                    {
                        new RequirementConfig { Item = "Joint_paper", Amount = 1 },
                        new RequirementConfig { Item = "weed_buds", Amount = 1 },
                        new RequirementConfig { Item = "Raspberry", Amount = 1 }
                    }
                });
            hj_rune.ItemDrop.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Consumable;
            hj_rune.ItemDrop.m_itemData.m_shared.m_consumeStatusEffect = hybridJointEffect.StatusEffect;
            ItemManager.Instance.AddItem(hj_rune);

            CustomItem ij_rune = new CustomItem(indicaJointPrefab, fixReference: false,
                new ItemConfig
                {
                    Amount = 1,
                    Requirements = new[]
                    {
                        new RequirementConfig { Item = "Joint_paper", Amount = 1 },
                        new RequirementConfig { Item = "weed_buds", Amount = 1 },
                        new RequirementConfig { Item = "Blueberries", Amount = 1 }
                    }
                });
            ij_rune.ItemDrop.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Consumable;
            ij_rune.ItemDrop.m_itemData.m_shared.m_consumeStatusEffect = indicaJointEffect.StatusEffect;
            ij_rune.ItemPrefab = indicaJointPrefab;
            ItemManager.Instance.AddItem(ij_rune);

            CustomItem sj_rune = new CustomItem(sativaJointPrefab, fixReference: false,
                new ItemConfig
                {
                    Amount = 1,
                    Requirements = new[]
                    {
                        new RequirementConfig { Item = "Joint_paper", Amount = 1 },
                        new RequirementConfig { Item = "weed_buds", Amount = 1 },
                        new RequirementConfig { Item = "Honey", Amount = 1 }
                    }
                });
            sj_rune.ItemDrop.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Consumable;
            sj_rune.ItemDrop.m_itemData.m_shared.m_consumeStatusEffect = sativaJointEffect.StatusEffect;
            sj_rune.ItemPrefab = sativaJointPrefab;
            ItemManager.Instance.AddItem(sj_rune);

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

        private void CreateRunePieces()
        {
            CustomPiece weedPlant = new CustomPiece(weedSaplingPrefab,
                new PieceConfig
                {
                    PieceTable = "Cultivator",
                    Requirements = new[]
                    {
                        new RequirementConfig { Item = "weed_seeds", Amount = 1 }
                    }
                });
            PieceManager.Instance.AddPiece(weedPlant);

            CustomPiece weedSeedPlant = new CustomPiece(weedSeedSaplingPrefab,
                new PieceConfig
                {
                    PieceTable = "Cultivator",
                    Requirements = new[]
                    {
                        new RequirementConfig { Item = "weed_buds", Amount = 1 }
                    }
                });
            PieceManager.Instance.AddPiece(weedSeedPlant);

            //CustomPiece bong = new CustomPiece(bongPrefab,
            //    new PieceConfig
            //    {
            //        PieceTable = "Hammer",
            //        Requirements = new[]
            //        {
            //            new RequirementConfig { Item = "Wood", Amount = 1 }
            //        }
            //    });
            //bong.PiecePrefab.AddComponent<Bong>();
            //Bong bongConfig = new Bong();
            //bongConfig.m_name = "$piece_bong";

            //PieceManager.Instance.AddPiece(bong);
        }
    }
}