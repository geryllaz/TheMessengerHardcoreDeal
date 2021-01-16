using Mod.Courier;
using Mod.Courier.Module;
using Mod.Courier.UI;

namespace TheHardcoreDeal
{
    public class HardcoreDealModule : CourierModule
    {
        private ToggleButtonInfo _hardcoreDealModeButton;
        private bool _hcMode = true;
        private bool _isNewGamePlus;
        public override void Load()
        {
            _hardcoreDealModeButton = Courier.UI.RegisterToggleModOptionButton(() => "Hardcore Deal", OnHardcoreModeToggle, b => _hcMode);

            On.SaveGameSelectionScreen.OnLoadGame += SaveGameSelectionScreen_OnLoadGame;

            On.InventoryManager.AddItem += InventoryManagerOnAddItem;

            On.PlayerController.ReceiveHit += PlayerController_ReceiveHit;
            On.Enemy.ReceiveHit += EnemyOnReceiveHit;
            On.TotemBoss.OnReceiveHit += TotemBossOnOnReceiveHit;

            On.Quarble.OnPlayerDied += QuarbleOnOnPlayerDied;
        }

        private void SaveGameSelectionScreen_OnLoadGame(On.SaveGameSelectionScreen.orig_OnLoadGame orig, SaveGameSelectionScreen self, int slotIndex)
        {
            orig(self, slotIndex);

            if (!_hcMode)
            {
                return;
            }

            if (Manager<SaveManager>.Instance.GetCurrentSaveGameSlot().SlotName.EndsWith("_hc"))
            {
                if (Manager<SkinManager>.Instance.GetEquippedSkin().skinID != ESkin.DARK_MESSENGER)
                {
                    Manager<SkinManager>.Instance.EquipSkin(ESkin.DARK_MESSENGER);
                }

                Manager<InventoryManager>.Instance.SetItemQuantity(EItems.TIME_SHARD, 0);
            }
            else
            {
                OnHardcoreModeToggle();
            }
        }

        private void QuarbleOnOnPlayerDied(On.Quarble.orig_OnPlayerDied orig, Quarble self, EDeathType deathType, bool fastReload)
        {
            orig(self, deathType, fastReload);

            if (!_hcMode)
            {
                return;
            }

            if (!_isNewGamePlus)
            {
                Manager<SaveManager>.Instance.NewGame();
            }
            else
            {
                Manager<SaveManager>.Instance.GetCurrentSaveGameSlot().ResetNewGamePlus();
                Manager<SaveManager>.Instance.SaveCurrentGameSlot(false);
            }
        }

        private void InventoryManagerOnAddItem(On.InventoryManager.orig_AddItem orig, InventoryManager self, EItems itemId, int quantity)
        {
            if (!_hcMode)
            {
                orig(self, itemId, quantity);
                return;
            }

            switch (itemId)
            {
                case EItems.BASIC_SCROLL:
                    Manager<SkinManager>.Instance.EquipSkin(ESkin.DARK_MESSENGER);
                    break;
                case EItems.TIME_SHARD:
                    orig(self, itemId, 0);
                    break;
                default:
                    orig(self, itemId, quantity);
                    break;
            }
        }

        private void TotemBossOnOnReceiveHit(On.TotemBoss.orig_OnReceiveHit orig, TotemBoss self, Hittable hittable, HitData hitData)
        {
            if (!_hcMode)
            {
                orig(self, hittable, hitData);
            }

            hitData.damage = 1;
            orig(self, hittable, hitData);
        }

        private void OnHardcoreModeToggle()
        {
            _hcMode = !_hcMode;
            _hardcoreDealModeButton.UpdateStateText();
        }

        private void PlayerController_ReceiveHit(On.PlayerController.orig_ReceiveHit orig, PlayerController self, HitData hitData)
        {
            if (!_hcMode)
            {
                orig(self, hitData);
                return;
            }

            Manager<InventoryManager>.Instance.SetItemQuantity(EItems.TIME_SHARD, 0);

            _isNewGamePlus = Manager<SaveManager>.Instance.GetCurrentSaveGameSlot().NewGamePlus;

            if (!_isNewGamePlus)
            {
                Manager<SaveManager>.Instance.GetCurrentSaveGameSlot().NewGamePlus = true;
            }

            hitData.damage = 1000;
            orig(self, hitData);
        }

        private bool EnemyOnReceiveHit(On.Enemy.orig_ReceiveHit orig, Enemy self, HitData hitData)
        {
            if (!_hcMode)
            {
                return orig(self, hitData);
            }

            hitData.damage = 1;
            return orig(self, hitData);
        }
    }
}
