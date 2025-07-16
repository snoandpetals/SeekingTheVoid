using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using RoR2.Items;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using VoidItemAPI;
using ShaderSwapper;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace SeekingTheVoid
{
    [BepInDependency(VoidItemAPI.VoidItemAPI.MODGUID)]
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    // This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class SeekingTheVoid : BaseUnityPlugin
    {
        // The Plugin GUID should be a unique ID for this plugin,
        // which is human readable (as it is used in places like the config).
        // If we see this PluginGUID as it is on thunderstore,
        // we will deprecate this mod.
        // Change the PluginAuthor and the PluginName !
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "acanthi";
        public const string PluginName = "SeekingTheVoid";
        public const string PluginVersion = "1.0.0";

        // We need our item definition to persist through our functions, and therefore make it a class field.
        private static ItemDef CoastalCoralDef;
        public static AssetBundle SeekingTheVoidAssets;

        protected void CreateLang()
        {
            LanguageAPI.Add("SEEKINTHEVOID_COASTALCORAL_NAME", "Coastal Coral");
            LanguageAPI.Add("SEEKINTHEVOID_COASTALCORAL_LORE", "TODO");
            LanguageAPI.Add("SEEKINTHEVOID_COASTALCORAL_PICKUP", "Slightly increase movement speed. Spawns orbs that reduce gravity. <style=cIsVoid>Corrupts all Elusive Antlers</style>.");
            LanguageAPI.Add("SEEKINTHEVOID_COASTALCORAL_DESC", "Increases movement speed by <style=cIsUtility>7%</style>. Spawns orbs nearby every <style=cIsUtility>10s</style> <style=cStack>(-10% per stack)</style>, giving <style=cIsUtility>-20% gravity</style> up to <style=cIsUtility>3</style> <style=cStack>(+3 per stack)</style> <style=cIsUtility>times</style> for <style=cIsUtility>12s</style>");
        }

        // The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            Log.Init(Logger);
            CreateLang();

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SeekingTheVoid.seekingthevoid_assets"))
            {
                SeekingTheVoidAssets = AssetBundle.LoadFromStream(stream);
            }

            // Upgrade Shaders :3
            base.StartCoroutine(SeekingTheVoidAssets.UpgradeStubbedShadersAsync());

            CoastalCoralDef = ScriptableObject.CreateInstance<ItemDef>();

            // Language Tokens, explained there https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Assets/Localization/
            CoastalCoralDef.name = "SEEKINTHEVOID_COASTALCORAL_NAME";
            CoastalCoralDef.nameToken = "SEEKINTHEVOID_COASTALCORAL_NAME";
            CoastalCoralDef.pickupToken = "SEEKINTHEVOID_COASTALCORAL_PICKUP";
            CoastalCoralDef.descriptionToken = "SEEKINTHEVOID_COASTALCORAL_DESC";
            CoastalCoralDef.loreToken = "SEEKINTHEVOID_COASTALCORAL_LORE";

            CoastalCoralDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/DLC1/Common/VoidTier1Def.asset").WaitForCompletion();

            CoastalCoralDef.requiredExpansion = Addressables.LoadAssetAsync<ExpansionDef>("RoR2/DLC1/Common/DLC1.asset").WaitForCompletion();

            // You can create your own icons and prefabs through assetbundles, but to keep this boilerplate brief, we'll be using question marks.
            CoastalCoralDef.pickupIconSprite = SeekingTheVoidAssets.LoadAsset<Sprite>("CoastalCoral.png");
            CoastalCoralDef.pickupModelPrefab = SeekingTheVoidAssets.LoadAsset<GameObject>("CoastalCoral.prefab");

            // You can add your own display rules here,
            // where the first argument passed are the default display rules:
            // the ones used when no specific display rules for a character are found.
            // For this example, we are omitting them,
            // as they are quite a pain to set up without tools like https://thunderstore.io/package/KingEnderBrine/ItemDisplayPlacementHelper/
            var displayRules = new ItemDisplayRuleDict(null);
            displayRules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = SeekingTheVoidAssets.LoadAsset<GameObject>("CoastalCoral.prefab"),
                    childName = "Head",
                    localPos = new Vector3(0.15F, 0.15F, -0.15F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.8F, 0.8F, 0.8F)
                }
            });
            displayRules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = SeekingTheVoidAssets.LoadAsset<GameObject>("CoastalCoral.prefab"),
                    childName = "Head",
                    localPos = new Vector3(0.15F, 0.15F, -0.15F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.8F, 0.8F, 0.8F)
                }
            });
            displayRules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = SeekingTheVoidAssets.LoadAsset<GameObject>("CoastalCoral.prefab"),
                    childName = "Head",
                    localPos = new Vector3(0.15F, 0.15F, -0.15F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.8F, 0.8F, 0.8F)
                }
            });
            displayRules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = SeekingTheVoidAssets.LoadAsset<GameObject>("CoastalCoral.prefab"),
                    childName = "Head",
                    localPos = new Vector3(0.15F, 0.15F, -0.15F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.8F, 0.8F, 0.8F)
                }
            });
            displayRules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = SeekingTheVoidAssets.LoadAsset<GameObject>("CoastalCoral.prefab"),
                    childName = "Head",
                    localPos = new Vector3(0.15F, 0.15F, -0.15F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.8F, 0.8F, 0.8F)
                }
            });
            displayRules.Add("mdlEngi", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = SeekingTheVoidAssets.LoadAsset<GameObject>("CoastalCoral.prefab"),
                    childName = "Head",
                    localPos = new Vector3(0.15F, 0.15F, -0.15F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.8F, 0.8F, 0.8F)
                }
            });
            displayRules.Add("mdlEngiTurret", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = SeekingTheVoidAssets.LoadAsset<GameObject>("CoastalCoral.prefab"),
                    childName = "Head",
                    localPos = new Vector3(0.15F, 0.15F, -0.15F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.8F, 0.8F, 0.8F)
                }
            });
            displayRules.Add("mdlMage", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = SeekingTheVoidAssets.LoadAsset<GameObject>("CoastalCoral.prefab"),
                    childName = "Head",
                    localPos = new Vector3(0.15F, 0.15F, -0.15F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.8F, 0.8F, 0.8F)
                }
            });
            displayRules.Add("mdlMerc", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = SeekingTheVoidAssets.LoadAsset<GameObject>("CoastalCoral.prefab"),
                    childName = "Head",
                    localPos = new Vector3(0.15F, 0.15F, -0.15F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.8F, 0.8F, 0.8F)
                }
            });
            displayRules.Add("mdlLoader", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = SeekingTheVoidAssets.LoadAsset<GameObject>("CoastalCoral.prefab"),
                    childName = "Head",
                    localPos = new Vector3(0.15F, 0.15F, -0.15F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.8F, 0.8F, 0.8F)
                }
            });
            displayRules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = SeekingTheVoidAssets.LoadAsset<GameObject>("CoastalCoral.prefab"),
                    childName = "Head",
                    localPos = new Vector3(0.15F, 0.15F, -0.15F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.8F, 0.8F, 0.8F)
                }
            });
            displayRules.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = SeekingTheVoidAssets.LoadAsset<GameObject>("CoastalCoral.prefab"),
                    childName = "Head",
                    localPos = new Vector3(0.15F, 0.15F, -0.15F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.8F, 0.8F, 0.8F)
                }
            });
            displayRules.Add("mdlSeeker", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = SeekingTheVoidAssets.LoadAsset<GameObject>("CoastalCoral.prefab"),
                    childName = "Head",
                    localPos = new Vector3(0.15F, 0.15F, -0.15F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.8F, 0.8F, 0.8F)
                }
            });
            displayRules.Add("mdlChef", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = SeekingTheVoidAssets.LoadAsset<GameObject>("CoastalCoral.prefab"),
                    childName = "Head",
                    localPos = new Vector3(0.15F, 0.15F, -0.15F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.8F, 0.8F, 0.8F)
                }
            });

            // Then finally add it to R2API
            ItemAPI.Add(new CustomItem(CoastalCoralDef, displayRules));

            On.RoR2.ItemCatalog.Init += AddVoidTransformation;
        }

        private void AddVoidTransformation(On.RoR2.ItemCatalog.orig_Init orig)
        {
            orig();

            ItemDef ElusiveAntlersItemDef = Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC2/Items/SpeedBoostPickup/SpeedBoostPickup.asset").WaitForCompletion();
            VoidItemAPI.VoidTransformation.CreateTransformation(CoastalCoralDef, ElusiveAntlersItemDef);
        }


        // The Update() method is run on every frame of the game.
        private void Update()
        {
            // This if statement checks if the player has currently pressed F2.
            if (Input.GetKeyDown(KeyCode.F5))
            {
                // Get the player body to use a position:
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                // And then drop our defined item in front of the player.

                Log.Info($"Player pressed F2. Spawning our custom item at coordinates {transform.position}");
                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(CoastalCoralDef.itemIndex), transform.position, transform.forward * 20f);
            }
        }

        // TODO
        // MAKE THIS INTO A NETWORK BEHAVIOUR
        // HAVE THE SCRIPT BE ATTACHED TO THE BODY ON INVENTORY UPDATE
        // THEN MAKE THIS NETWORKED! YOU MORON!
        public class CoastalCoralBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnServer = true, useOnClient = false)]
            private static ItemDef GetItemDef() { return CoastalCoralDef; }
            private int GetStackCount() { return stack; }

            private float internalTimer = 10f;
            

            private void OnEnable()
            {
            }

            private void OnDisable()
            {
            }

            public void FixedUpdate()
            {
                if (NetworkServer.active && (bool)body && stack > 0) {
                    internalTimer -= Time.fixedDeltaTime;
                    if (internalTimer <= 0)
                    {
                        OrbPositionFinder();
                        internalTimer = 10f;
                    }
                }
            }

            public void OrbPositionFinder()
            {
                Quaternion quaternion = Quaternion.Euler(0f, UnityEngine.Random.Range(-45f, 45f), 0f);
                Vector3 vector = new Vector3(body.inputBank.aimDirection.x, 0f, body.inputBank.aimDirection.z);
                Vector3 velocity = body.characterMotor.velocity;
                float magnitude = velocity.magnitude;
                float t = Mathf.InverseLerp(0f, 50f, magnitude);
                float num = Mathf.Lerp(1f, 2f, t);
                Vector3 current = quaternion * vector;
                current = Vector3.RotateTowards(current, velocity, MathF.PI / 12f, 0f);
                Vector3 position = base.gameObject.transform.position + current * (UnityEngine.Random.Range(15f, 30f) * num);
                NodeGraph groundNodes = SceneInfo.instance.groundNodes;
                NodeGraph.NodeIndex nodeIndex = groundNodes.FindClosestNode(position, HullClassification.Human);
                if (groundNodes.GetNodePosition(nodeIndex, out position))
                {
                    //i think changing it here would work
                    float num2 = HullDef.Find(HullClassification.Human).radius * 0.7f;
                    if (!HGPhysics.DoesOverlapSphere(position + Vector3.up * (num2 + 0.25f), num2, (int)LayerIndex.world.mask | (int)LayerIndex.defaultLayer.mask | (int)LayerIndex.CommonMasks.fakeActorLayers | (int)LayerIndex.entityPrecise.mask | (int)LayerIndex.debris.mask))
                    {
                        SpawnShit(position);
                    }
                }

                //DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(VoidCampSpawnCard, new DirectorPlacementRule
                //{
                //    placementMode = DirectorPlacementRule.PlacementMode.NearestNode,
                //    position = position
                //}, rng));
            }

            public void SpawnShit(Vector3 position)
            {
                GameObject gameObject = Instantiate(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/FalseSon/FalseSonBody.prefab").WaitForCompletion(), position, Quaternion.identity);
                NetworkServer.Spawn(gameObject);
            }

        }
    }
}
