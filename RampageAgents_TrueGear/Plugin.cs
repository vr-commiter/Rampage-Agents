using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using HarmonyLib;
using MTFrame;
using MTFrame.Abilitys;
using MTFrame.GameContents;
using MTFrame.Model;
using MTFrame.Model.Functions;
using MTFrame.Model.Items.Medicaments;
using MTFrame.Model.PlayerAI;
using MTFrame.Model.Tools;
using MTFrame.Network;
using MTFrame.Tools;
using MTFrame.UI;
using MTFrame.VR;
using MyTrueGear;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using MTFrame.UI;
using static UnityEngine.UIElements.StyleSheets.Dimension;

namespace RampageAgents_TrueGear
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;

        private static BodyPositionStatus lastBodyPositionStatus = BodyPositionStatus.无;
        private static bool canHandBind = false;

        private static string leftHandItemID = null;
        private static string rightHandItemID = null;
        private static string localPlayerID = null;

        private static bool canRope = true;
        private static bool canBlastingToolShake = true;
        private static bool canBlastingToolPress = true;

        private static TrueGearMod _TrueGear = null;


        private void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;
            _TrueGear = new TrueGearMod();
            Harmony.CreateAndPatchAll(typeof(Plugin));

            _TrueGear.Play("HeartBeat");
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }

        public static KeyValuePair<float, float> GetAngle(Transform player, Vector3 hit)
        {
            // 计算玩家和击中点之间的向量
            Vector3 direction = hit - player.position;

            // 计算玩家正前方向量在水平面上的投影
            Vector3 forward = Vector3.ProjectOnPlane(player.forward, Vector3.up).normalized;

            // 计算夹角
            float angle = Vector3.SignedAngle(forward, direction, Vector3.up);

            angle = 360f - ((angle + 360f) % 360f);

            // 计算垂直偏移量
            float verticalOffset = player.transform.position.y - hit.y;

            return new KeyValuePair<float, float>(angle, verticalOffset);
        }



        [HarmonyPostfix, HarmonyPatch(typeof(GrapplingRope), "DrawRope")]
        private static void GrapplingRope_DrawRope_Prefix(GrapplingRope __instance)
        {
            if (!__instance.flyingClaw.IsEnabled || !__instance.flyingClaw.IsShowLine())
            {
                return;
            }
            if (__instance.flyingClaw.m_origin.ID != localPlayerID)
            {
                return;
            }
            if (__instance.flyingClaw.IsGrappling())
            {
                if (!canRope)
                {
                    return;
                }
                canRope = false;
                Timer ropeTimer = new Timer(RopeTimerCallBack, null, 130, Timeout.Infinite);
                Logger.LogInfo("------------------------------------");
                Logger.LogInfo("LeftHandDrawRope");
                _TrueGear.Play("LeftHandDrawRope");
                Logger.LogInfo(__instance.grapplingRopeType);
                Logger.LogInfo(__instance.flyingClaw.IsOwner);
            }
        }

        private static void RopeTimerCallBack(object o)
        {
            canRope = true;
        }


        [HarmonyPostfix, HarmonyPatch(typeof(BaseGun), "Shoot")]
        private static void BaseGun_Shoot_Postfix(BaseGun __instance, string bulletID)
        {
            if (!__instance.IsOwner)
            {
                return;
            }
            if (string.IsNullOrEmpty(bulletID) || bulletID == "B_8000")
            {
                return;
            }
            if (__instance.GunType == GunSystemEnumType.玩家技能)
            {
                Logger.LogInfo("------------------------------------");
                Logger.LogInfo("SkillShoot");
                _TrueGear.Play("SkillShoot");
                return;
            }
            if (__instance.IsBothHand)
            {
                if (__instance.GunType == GunSystemEnumType.冲锋枪 || __instance.GunType == GunSystemEnumType.步枪 || __instance.GunType == GunSystemEnumType.喷射器)
                {
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo("LeftHandRifleShoot");
                    Logger.LogInfo("RightHandRifleShoot");
                    _TrueGear.Play("LeftHandRifleShoot");
                    _TrueGear.Play("RightHandRifleShoot");
                }
                else if (__instance.GunType == GunSystemEnumType.霰弹枪 || __instance.GunType == GunSystemEnumType.狙击枪 || __instance.GunType == GunSystemEnumType.发射器)
                {
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo("LeftHandShotgunShoot");
                    Logger.LogInfo("RightHandShotgunShoot");
                    _TrueGear.Play("LeftHandShotgunShoot");
                    _TrueGear.Play("RightHandShotgunShoot");
                }
                else
                {
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo("LeftHandPistolShoot");
                    Logger.LogInfo("RightHandPistolShoot");
                    _TrueGear.Play("LeftHandPistolShoot");
                    _TrueGear.Play("RightHandPistolShoot");
                }
            }
            else
            {
                if (__instance.Hands[0].left)
                {
                    if (__instance.GunType == GunSystemEnumType.冲锋枪 || __instance.GunType == GunSystemEnumType.步枪 || __instance.GunType == GunSystemEnumType.喷射器)
                    {
                        Logger.LogInfo("------------------------------------");
                        Logger.LogInfo("LeftHandRifleShoot");
                        _TrueGear.Play("LeftHandRifleShoot");
                    }
                    else if (__instance.GunType == GunSystemEnumType.霰弹枪 || __instance.GunType == GunSystemEnumType.狙击枪 || __instance.GunType == GunSystemEnumType.发射器)
                    {
                        Logger.LogInfo("------------------------------------");
                        Logger.LogInfo("LeftHandShotgunShoot");
                        _TrueGear.Play("LeftHandShotgunShoot");
                    }
                    else
                    {
                        Logger.LogInfo("------------------------------------");
                        Logger.LogInfo("LeftHandPistolShoot");
                        _TrueGear.Play("LeftHandPistolShoot");
                    }
                }
                else
                {
                    if (__instance.GunType == GunSystemEnumType.冲锋枪 || __instance.GunType == GunSystemEnumType.步枪 || __instance.GunType == GunSystemEnumType.喷射器)
                    {
                        Logger.LogInfo("------------------------------------");
                        Logger.LogInfo("RightHandRifleShoot");
                        _TrueGear.Play("RightHandRifleShoot");
                    }
                    else if (__instance.GunType == GunSystemEnumType.霰弹枪 || __instance.GunType == GunSystemEnumType.狙击枪 || __instance.GunType == GunSystemEnumType.发射器)
                    {
                        Logger.LogInfo("------------------------------------");
                        Logger.LogInfo("RightHandShotgunShoot");
                        _TrueGear.Play("RightHandShotgunShoot");
                    }
                    else
                    {
                        Logger.LogInfo("------------------------------------");
                        Logger.LogInfo("RightHandPistolShoot");
                        _TrueGear.Play("RightHandPistolShoot");
                    }
                }
            }
            Logger.LogInfo(__instance.GunType);
            Logger.LogInfo(__instance.IsOwner);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BaseGun), "OnGrabMainHandle")]
        private static void BaseGun_OnGrabMainHandle_Postfix(BaseGun __instance, HandleGun handle)
        {
            if (!__instance.IsOwner)
            {
                return;
            }
            if (handle.Hand_IK.left)
            {
                Logger.LogInfo("------------------------------------");
                Logger.LogInfo("LeftHandPickupItem");
                _TrueGear.Play("LeftHandPickupItem");
            }
            else
            {
                Logger.LogInfo("------------------------------------");
                Logger.LogInfo("RightHandPickupItem");
                _TrueGear.Play("RightHandPickupItem");
            }
        }

        private static string gunID = null;

        [HarmonyPostfix, HarmonyPatch(typeof(BaseGun), "OnReleaseMainHandle")]
        private static void BaseGun_OnReleaseMainHandle_Postfix(BaseGun __instance, HandleGun handle)
        {
            if (!__instance.IsOwner)
            {
                return;
            }
            gunID = __instance.ID;
            Logger.LogInfo("------------------------------------");
            Logger.LogInfo($"gunID :{gunID}");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BaseGun), "OnGrabHandle")]
        private static void BaseGun_OnGrabHandle_Postfix(BaseGun __instance, HandleGun handle)
        {
            if (!__instance.IsOwner)
            {
                return;
            }
            if (handle.Hand_IK.left)
            {
                Logger.LogInfo("------------------------------------");
                Logger.LogInfo("LeftHandPickupItem");
                _TrueGear.Play("LeftHandPickupItem");
            }
            else
            {
                Logger.LogInfo("------------------------------------");
                Logger.LogInfo("RightHandPickupItem");
                _TrueGear.Play("RightHandPickupItem");
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BaseGun), "OnGrabBolt")]
        private static void BaseGun_OnGrabBolt_Postfix(BaseGun __instance)
        {
            if (!__instance.IsOwner)
            {
                return;
            }
            if (__instance.Hands[0].left)
            {
                Logger.LogInfo("------------------------------------");
                Logger.LogInfo("RightHandPickupItem");
                _TrueGear.Play("RightHandPickupItem");
            }
            else
            {
                Logger.LogInfo("------------------------------------");
                Logger.LogInfo("LeftHandPickupItem");
                _TrueGear.Play("LeftHandPickupItem");
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BaseGun), "OnReleaseBolt")]
        private static void BaseGun_OnReleaseBolt_Postfix(BaseGun __instance)
        {

            if (!__instance.IsOwner)
            {
                return;
            }
            if (__instance.GunStateType == GunStateType.填充子弹完成)
            {
                if (__instance.Hands[0].left)
                {
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo("LeftHandBulletFilled");
                    _TrueGear.Play("LeftHandBulletFilled");
                }
                else
                {
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo("RightHandBulletFilled");
                    _TrueGear.Play("RightHandBulletFilled");
                }         
            }
        }


        //[HarmonyPostfix, HarmonyPatch(typeof(NetworkPlayer), "OnStartServer")]
        //private static void NetworkPlayer_OnStartServer_Postfix(NetworkPlayer __instance)
        //{
        //    if (!__instance._isLocal)
        //    {
        //        return;
        //    }
        //    Logger.LogInfo("------------------------------------");
        //    Logger.LogInfo("OnStartServer");
        //    Logger.LogInfo(__instance._isLocal);
        //    localPlayerID = __instance.ID;
        //    //__instance._playerModelAgent.OnHitedEvent.AddListener(new Action<BuffTypeId, DamageResult>(TrueGearHitedEvent));
        //    __instance._playerModelAgent.OnDeathEvent.AddListener(new Action<string, BuffTypeId, DamageResult>(TrueGearDeathEvent));
        //}


        //[HarmonyPostfix, HarmonyPatch(typeof(NetworkPlayer), "OnStopServer")]
        //private static void NetworkPlayer_OnStopServer_Postfix(NetworkPlayer __instance)
        //{
        //    if (!__instance._isLocal)
        //    {
        //        return;
        //    }
        //    Logger.LogInfo("------------------------------------");
        //    Logger.LogInfo("OnStopServer");
        //    Logger.LogInfo(__instance._isLocal);
        //    //__instance._playerModelAgent.OnHitedEvent.RemoveListener(new Action<BuffTypeId, DamageResult>(TrueGearHitedEvent));
        //    __instance._playerModelAgent.OnDeathEvent.RemoveListener(new Action<string, BuffTypeId, DamageResult>(TrueGearDeathEvent));
        //}

        //private static void TrueGearDeathEvent(string id, BuffTypeId buffTypeId, DamageResult damageData)
        //{
        //    Logger.LogInfo("------------------------------------");
        //    Logger.LogInfo("PlayerDeath");
        //}

        //private static void TrueGearHitedEvent(BuffTypeId buffTypeId, DamageResult damageData)
        //{
        //    var angle = GetAngle(damageData.UseData.StartPosition, damageData.UseData.EndPosition, damageData.UseData.HitPoint);
        //    Logger.LogInfo("------------------------------------");
        //    Logger.LogInfo($"TrueGearHitedEvent,{angle.Key},{angle.Value}");


        //    Logger.LogInfo($"StartPos :{damageData.UseData.StartPosition.x},{damageData.UseData.StartPosition.y},{damageData.UseData.StartPosition.z}");
        //    Logger.LogInfo($"EndPos :{damageData.UseData.EndPosition.x},{damageData.UseData.EndPosition.y},{damageData.UseData.EndPosition.z}");
        //    Logger.LogInfo($"HitPoint :{damageData.UseData.HitPoint.x},{damageData.UseData.HitPoint.y},{damageData.UseData.HitPoint.z}");
        //    Logger.LogInfo($"WeaponId :{damageData.WeaponId}");
        //    Logger.LogInfo($"AttackerId :{damageData.AttackerId}");
        //    Logger.LogInfo($"BuffTypeId :{damageData.BuffData.BuffTypeId}");
        //}


        //[HarmonyPostfix, HarmonyPatch(typeof(StatusMachineBase), "OnStatusChanged")]
        //private static void StatusMachineBase_OnStatusChanged_Postfix(StatusMachineBase __instance)
        //{
        //    Logger.LogInfo("------------------------------------");
        //    Logger.LogInfo("StatusMachineBaseOnStatusChanged");
        //}

        //[HarmonyPostfix, HarmonyPatch(typeof(PlayerStatusSyncFunc), "OnStatusChanged")]
        //private static void PlayerStatusSyncFunc_OnStatusChanged_Postfix(PlayerStatusSyncFunc __instance, StatusType last, StatusType current)
        //{
        //    if (!__instance.m_networkPlayer._isLocal)
        //    {
        //        return;
        //    }
        //    Logger.LogInfo("------------------------------------");
        //    Logger.LogInfo("OnStatusChanged");
        //    Logger.LogInfo(last);
        //    Logger.LogInfo(current);
        //    Logger.LogInfo(__instance.m_networkPlayer._isLocal);
        //}


        [HarmonyPostfix, HarmonyPatch(typeof(NetworkPlayer), "OnStatusChanged")]
        private static void NetworkPlayer_OnStatusChanged_Postfix(NetworkPlayer __instance, StatusType oldValue, StatusType newValue)
        {
            if (__instance._isLocal || __instance.IsLocal)
            {
                Logger.LogInfo("------------------------------------");
                if (newValue == StatusType.Alive)
                {
                    Logger.LogInfo("PlayerAlive");
                    _TrueGear.Play("PlayerAlive");
                    localPlayerID = __instance.ID;
                }
                if (newValue == StatusType.Death)
                {
                    Logger.LogInfo("PlayerDeath");
                    _TrueGear.Play("PlayerDeath");
                }
                Logger.LogInfo("OnStatusChanged");
                Logger.LogInfo(__instance._isLocal);
                Logger.LogInfo(oldValue);
                Logger.LogInfo(newValue);
                Logger.LogInfo(__instance.ID);
                Logger.LogInfo(__instance.NetworkID);
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(NetworkPlayer), "OnHealthPointChanged")]
        private static void NetworkPlayer_OnHealthPointChanged_Postfix(NetworkPlayer __instance, float oldValue, float newValue)
        {
            if (__instance._isLocal || __instance.IsLocal)
            {
                if (oldValue + 1f < newValue)
                {
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo("Healing");
                    _TrueGear.Play("Healing");
                    Logger.LogInfo(oldValue);
                    Logger.LogInfo(newValue);
                }
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(NetworkPlayer), "OnShieldPointChanged")]
        private static void NetworkPlayer_OnShieldPointChanged_Postfix(NetworkPlayer __instance, float oldValue, float newValue)
        {
            if (__instance._isLocal || __instance.IsLocal)
            {
                if (oldValue + 1f < newValue)
                {
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo("ShieldCharge");
                    _TrueGear.Play("ShieldCharge");
                    Logger.LogInfo(oldValue);
                    Logger.LogInfo(newValue);
                }
            }
        }

        //[HarmonyPostfix, HarmonyPatch(typeof(NetworkPlayer), "OnTreateEvent")]
        //private static void NetworkPlayer_OnTreateEvent_Postfix(NetworkPlayer __instance, string id, BuffTypeId buffTypeId, DamageResult damageData)
        //{
        //    if (!__instance._isLocal)
        //    {
        //        return;
        //    }
        //    Logger.LogInfo("------------------------------------");
        //    Logger.LogInfo("OnTreateEvent");
        //    Logger.LogInfo(__instance._isLocal);
        //    Logger.LogInfo(buffTypeId);
        //}

        [HarmonyPostfix, HarmonyPatch(typeof(NetworkPlayer), "PickupModel")]
        private static void NetworkPlayer_PickupModel_Postfix(NetworkPlayer __instance,bool __result, IPickedable model, PickupType pickupType)
        {
            if (__instance._isLocal || __instance.IsLocal)
            {
                if (pickupType == PickupType.LeftHand)
                {
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo("LeftHandPickupItem");
                    _TrueGear.Play("LeftHandPickupItem");
                    leftHandItemID = model.ID;
                }
                else
                {
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo("RightHandPickupItem");
                    _TrueGear.Play("RightHandPickupItem");
                    rightHandItemID = model.ID;
                }

                Logger.LogInfo(__instance._isLocal);
                Logger.LogInfo(pickupType);
                Logger.LogInfo(__result);
                Logger.LogInfo(model.ID);
            }

        }

        //[HarmonyPostfix, HarmonyPatch(typeof(NetworkPlayer), "CmdSetHandGrip")]
        //private static void NetworkPlayer_CmdSetHandGrip_Postfix(NetworkPlayer __instance, bool left, int index, float grip)
        //{
        //    if (!__instance._isLocal)
        //    {
        //        return;
        //    }
        //    Logger.LogInfo("------------------------------------");
        //    Logger.LogInfo("CmdSetHandGrip");
        //    Logger.LogInfo(__instance._isLocal);
        //    Logger.LogInfo(left);
        //    Logger.LogInfo(index);
        //    Logger.LogInfo(grip);
        //}

        [HarmonyPostfix, HarmonyPatch(typeof(NetworkPlayer), "UserCode_CmdSkillBoxUse")]
        private static void NetworkPlayer_UserCode_CmdSkillBoxUse_Postfix(NetworkPlayer __instance)
        {
            if (__instance._isLocal || __instance.IsLocal)
            {
                Logger.LogInfo("------------------------------------");
                Logger.LogInfo("UseSkill");
                _TrueGear.Play("UseSkill");
                Logger.LogInfo(__instance._isLocal);
            }

        }

        //[HarmonyPostfix, HarmonyPatch(typeof(NetworkPlayer), "UserCode_CmdItemUse")]
        //private static void NetworkPlayer_UserCode_CmdItemUse_Postfix(NetworkPlayer __instance, PickupType pickupType)
        //{
        //    if (!__instance._isLocal)
        //    {
        //        return;
        //    }
        //    Logger.LogInfo("------------------------------------");
        //    Logger.LogInfo("SelectedItem");
        //    Logger.LogInfo(__instance._isLocal);
        //    Logger.LogInfo(pickupType);
        //}

        [HarmonyPrefix, HarmonyPatch(typeof(Syringe), "HandBind")]
        private static void Syringe_HandBind_Prefix(Syringe __instance, IHand hand)
        {
            if (__instance.m_origin.ID != localPlayerID)
            {
                return;
            }
            if (__instance.m_followHand == null)
            {
                canHandBind = true;
            }             
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Syringe), "HandBind")]
        private static void Syringe_HandBind_Postfix(Syringe __instance, IHand hand)
        {
            if (__instance.m_origin.ID != localPlayerID)
            {
                return;
            }
            if (canHandBind)
            {
                canHandBind = false;
                if (__instance.m_followHand.Type == PickupType.LeftHand)
                {
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo("LeftHandBindSyringe");
                    _TrueGear.Play("LeftHandBindSyringe");
                }
                else
                {
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo("RightHandBindSyringe");
                    _TrueGear.Play("RightHandBindSyringe");
                }
                if (__instance.m_origin != null)
                {
                    if (__instance.m_followHand.Type == PickupType.LeftHand)
                    {
                        Logger.LogInfo("------------------------------------");
                        Logger.LogInfo("StartLeftHandSyringe");
                        _TrueGear.StartLeftHandSyringe();
                        return;
                    }
                    if (__instance.m_followHand.Type == PickupType.RightHand)
                    {
                        Logger.LogInfo("------------------------------------");
                        Logger.LogInfo("StartRightHandSyringe");
                        _TrueGear.StartRightHandSyringe();
                    }
                }
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Syringe), "HandUnbind")]
        private static void Syringe_HandUnbind_Prefix(Syringe __instance)
        {
            if (__instance.m_origin.ID != localPlayerID)
            {
                return;
            }
            if (__instance.m_followHand != null)
            {
                Logger.LogInfo("------------------------------------");
                Logger.LogInfo("StopSyringe");
                _TrueGear.StopSyringe();
                Logger.LogInfo(__instance.m_followHand.Type);
            }               
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Syringe), "OnItemPressed")]
        private static void Syringe_OnItemPressed_Prefix(Syringe __instance, string itemId, string ownerId)
        {
            if (__instance.m_origin.ID != localPlayerID)
            {
                return;
            }
            if (!__instance.m_isPress)
            {
                if (__instance.m_followHand.Type == PickupType.LeftHand)
                {
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo("StartLeftHandSyringe");
                    _TrueGear.StartLeftHandSyringe();
                }
                else
                {
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo("StartRightHandSyringe");
                    _TrueGear.StartRightHandSyringe();
                }

                Logger.LogInfo(itemId);
                Logger.LogInfo(ownerId);
            }
        }





        [HarmonyPostfix, HarmonyPatch(typeof(BulletManagerHandler), "GrenadeUsed")]
        private static void BulletManagerHandler_GrenadeUsed_Postfix(BulletManagerHandler __instance, GrenadeModel.UseData data)
        {
            if (!__instance.IsLocal)
            {
                return;
            }
            if (data.Mode == GrenadeMode.抛出)
            {
                if (isLeftGrenade)
                {
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo("LeftHandGrenadeThrow");
                    _TrueGear.Play("LeftHandGrenadeThrow");
                }
                else
                {
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo("RightHandGrenadeThrow");
                    _TrueGear.Play("RightHandGrenadeThrow");
                }
            }

        }

        private static bool isLeftGrenade = false;
        [HarmonyPostfix, HarmonyPatch(typeof(GrenadeModel), "OnPress")]
        private static void GrenadeModel_OnPress_Postfix(GrenadeModel __instance)
        {
            if (leftHandItemID == __instance.ID)
            {
                Logger.LogInfo("------------------------------------");
                Logger.LogInfo("LeftHandGrenadePress");
                _TrueGear.Play("LeftHandGrenadePress");
                isLeftGrenade = true;
            }
            else if (rightHandItemID == __instance.ID)
            {
                Logger.LogInfo("------------------------------------");
                Logger.LogInfo("RightHandGrenadePress");
                _TrueGear.Play("RightHandGrenadePress");
                isLeftGrenade = false;
            }
            Logger.LogInfo(__instance.ID);
            Logger.LogInfo(__instance.ItemID);
            Logger.LogInfo(leftHandItemID);
            Logger.LogInfo(rightHandItemID);            
        }

        //[HarmonyPostfix, HarmonyPatch(typeof(BlastingTool), "OnPress")]
        //private static void BlastingTool_OnPress_Postfix(BlastingTool __instance)
        //{
        //    SprayMessage sprayMessage = __instance.m_helper.CheckTarget();
        //    Logger.LogInfo("------------------------------------");
        //    Logger.LogInfo("BlastingToolOnPress");
        //    Logger.LogInfo(sprayMessage.PUBGDoor != null);
        //    Logger.LogInfo(__instance.gameObject.name);
        //}

        //[HarmonyPostfix, HarmonyPatch(typeof(BlastingTool), "OnUnpress")]
        //private static void BlastingTool_OnUnpress_Postfix(BlastingTool __instance)
        //{
        //    Logger.LogInfo("------------------------------------");
        //    Logger.LogInfo("BlastingToolOnUnpress");
        //    Logger.LogInfo(__instance._isSpray);
        //}

        [HarmonyPostfix, HarmonyPatch(typeof(BlastingTool), "Update")]
        private static void BlastingTool_Update_Postfix(BlastingTool __instance)
        {
            if (__instance.ID != leftHandItemID && __instance.ID != rightHandItemID)
            {
                return;
            }
            if (__instance.m_shakeAudio != null)
            {
                if (!canBlastingToolShake)
                {
                    return;
                }
                canBlastingToolShake = false;
                Timer blastingToolShakeTimer = new Timer(BlastingToolShakeTimerCallBack,null,120,Timeout.Infinite);
                if (__instance.ID == leftHandItemID)
                {
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo("LeftHandBlastingToolShake");
                    _TrueGear.Play("LeftHandBlastingToolShake");
                }
                else
                {
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo("RightHandBlastingToolShake");
                    _TrueGear.Play("RightHandBlastingToolShake");
                }

                Logger.LogInfo(__instance.gameObject.name);
                Logger.LogInfo(__instance.ItemID);
                Logger.LogInfo(__instance.ID);
                Logger.LogInfo(leftHandItemID);
                Logger.LogInfo(rightHandItemID);
            }
            if (__instance.m_sprayAudio != null)
            {
                if (!canBlastingToolPress)
                {
                    return;
                }
                canBlastingToolPress = false;
                Timer blastingToolPressTimer = new Timer(BlastingToolPressTimerCallBack, null, 130, Timeout.Infinite);
                if (__instance.ID == leftHandItemID)
                {
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo("LeftHandBlastingToolPress");
                    _TrueGear.Play("LeftHandBlastingToolPress");
                }
                else
                {
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo("RightHandBlastingToolPress");
                    _TrueGear.Play("RightHandBlastingToolPress");
                }
            }            
        }

        private static void BlastingToolShakeTimerCallBack(object o)
        {
            canBlastingToolShake = true;
        }

        private static void BlastingToolPressTimerCallBack(object o)
        {
            canBlastingToolPress = true;
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(BlastingTool), "OnGrab")]
        private static void BlastingTool_OnGrab_Postfix(BlastingTool __instance, HandleGun handle)
        {
            if (handle.Hand_IK.left)
            {
                Logger.LogInfo("------------------------------------");
                Logger.LogInfo("LeftHandPickupItem");
                _TrueGear.Play("LeftHandPickupItem");
                leftHandItemID = __instance.ID;
            }
            else
            {
                Logger.LogInfo("------------------------------------");
                Logger.LogInfo("RightHandPickupItem");
                _TrueGear.Play("RightHandPickupItem");
                rightHandItemID = __instance.ID;
            }
            
        }




        //[HarmonyPostfix, HarmonyPatch(typeof(NetworkPlayer), "OnDyingRescueChanged")]
        //private static void NetworkPlayer_OnDyingRescueChanged_Postfix(NetworkPlayer __instance, int oldValue, int newValue)
        //{
        //    Logger.LogInfo("------------------------------------");
        //    Logger.LogInfo("OnDyingRescueChanged");
        //    Logger.LogInfo(__instance._isLocal);
        //    Logger.LogInfo(oldValue);
        //    Logger.LogInfo(newValue);
        //}

        //[HarmonyPostfix, HarmonyPatch(typeof(NetworkPlayer), "UserCode_RpcPlayRescue")]
        //private static void NetworkPlayer_UserCode_RpcPlayRescue_Postfix(NetworkPlayer __instance)
        //{
        //    if (!__instance._isLocal)
        //    {
        //        return;
        //    }
        //    Logger.LogInfo("------------------------------------");
        //    Logger.LogInfo("UserCode_RpcPlayRescue");
        //    Logger.LogInfo(__instance._isLocal);
        //}

        //[HarmonyPostfix, HarmonyPatch(typeof(NetworkPlayer), "UserCode_RpcStopRescue")]
        //private static void NetworkPlayer_UserCode_RpcStopRescue_Postfix(NetworkPlayer __instance)
        //{
        //    if (!__instance._isLocal)
        //    {
        //        return;
        //    }
        //    Logger.LogInfo("------------------------------------");
        //    Logger.LogInfo("UserCode_RpcStopRescue");
        //    Logger.LogInfo(__instance._isLocal);
        //}


        //[HarmonyPostfix, HarmonyPatch(typeof(CourseRescueTask), "OnMoveRescuePosEnd")]
        //private static void CourseRescueTask_OnMoveRescuePosEnd_Postfix(CourseRescueTask __instance)
        //{

        //    Logger.LogInfo("------------------------------------");
        //    Logger.LogInfo("OnMoveRescuePosEnd");
        //    Logger.LogInfo(__instance._moveRescuePosTask);
        //}


        [HarmonyPostfix, HarmonyPatch(typeof(CourseRescueTarget), "CloseRescue")]
        private static void CourseRescueTarget_CloseRescue_Postfix(CourseRescueTarget __instance)
        {
            if (__instance.origin.ID != localPlayerID)
            {
                return;
            }
            Logger.LogInfo("------------------------------------");
            Logger.LogInfo("CloseRescue");
            _TrueGear.StopLeftHandCourseRescue();
            _TrueGear.StopRightHandCourseRescue();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(CourseRescueTarget), "Update")]
        private static void CourseRescueTarget_Update_Postfix(CourseRescueTarget __instance)
        {
            if (__instance.origin.ID != localPlayerID)
            {
                return;
            }
            if (!__instance.isCompleteRescue)
            {
                if (__instance.isOpenRescue)
                {
                    bool flag2 = false;
                    if (__instance.courseState.CurrentPlayerModel.EquipSlotRole.LeftPickedObjId == null || __instance.courseState.CurrentPlayerModel.EquipSlotRole.LeftPickedObjId == "")
                    {
                        int overlapSphere = Singleton<RayTools>.Instance.GetOverlapSphere(__instance.origin.LeftHand.transform.position, __instance.radius, __instance.leftColliders, __instance.layer);
                        for (int i = 0; i < overlapSphere; i++)
                        {
                            if (__instance.leftColliders[i].GetComponentInParent<CourseRescueTarget>() != null)
                            {
                                flag2 = true;
                                break;
                            }
                        }
                    }
                    bool flag3 = false;
                    if (__instance.courseState.CurrentPlayerModel.EquipSlotRole.RightPickedObjId == null || __instance.courseState.CurrentPlayerModel.EquipSlotRole.RightPickedObjId == "")
                    {
                        int overlapSphere2 = Singleton<RayTools>.Instance.GetOverlapSphere(__instance.origin.RightHand.transform.position, __instance.radius, __instance.rightColliders, __instance.layer);
                        for (int j = 0; j < overlapSphere2; j++)
                        {
                            if (__instance.rightColliders[j].GetComponentInParent<CourseRescueTarget>() != null)
                            {
                                flag3 = true;
                                break;
                            }
                        }
                    }

                    if (flag2 && __instance.leftHandHaptic == null)
                    {
                        Logger.LogInfo("------------------------------------");
                        Logger.LogInfo("StartLeftHandCourseRescue");
                        _TrueGear.StartLeftHandCourseRescue();
                    }
                    else if (!flag2)
                    {
                        HapticObject hapticObject = __instance.leftHandHaptic;
                        if (hapticObject != null)
                        {
                            Logger.LogInfo("------------------------------------");
                            Logger.LogInfo("StopLeftHandCourseRescue");
                            _TrueGear.StopLeftHandCourseRescue();
                        }
                    }
                    if (flag3 && __instance.rightHandHaptic == null)
                    {
                        Logger.LogInfo("------------------------------------");
                        Logger.LogInfo("StartRightHandCourseRescue");
                        _TrueGear.StartRightHandCourseRescue();
                    }
                    else if (!flag3)
                    {
                        HapticObject hapticObject2 = __instance.rightHandHaptic;
                        if (hapticObject2 != null)
                        {
                            Logger.LogInfo("------------------------------------");
                            Logger.LogInfo("StopRightHandCourseRescue");
                            _TrueGear.StopRightHandCourseRescue();
                        }
                    }
                }
            }
                   
        }



        [HarmonyPostfix, HarmonyPatch(typeof(PlayerModel), "SetBodyPositionStatus")]
        private static void PlayerModel_SetBodyPositionStatus_Postfix(PlayerModel __instance, BodyPositionStatus bodyPositionStatus)
        {
            if (__instance.ID != localPlayerID)
            {
                return;
            }
            if (bodyPositionStatus == BodyPositionStatus.蹲下)
            {
                Logger.LogInfo("------------------------------------");
                Logger.LogInfo("Crouch");
                _TrueGear.Play("Crouch");
            }
            else if (bodyPositionStatus == BodyPositionStatus.跳起)
            {
                Logger.LogInfo("------------------------------------");
                Logger.LogInfo("Jump");
                //_TrueGear.Play("Jump");
            }
            //else if (bodyPositionStatus == BodyPositionStatus.站立 && lastBodyPositionStatus == BodyPositionStatus.跳起)
            //{
            //    Logger.LogInfo("------------------------------------");
            //    Logger.LogInfo("Fall");
            //    _TrueGear.Play("Fall");
            //}
            lastBodyPositionStatus = bodyPositionStatus;
            Logger.LogInfo(bodyPositionStatus);
        }






        //[HarmonyPostfix, HarmonyPatch(typeof(EquipSlotWeapon), "OnAdd")]
        //private static void EquipSlotWeapon_OnAdd_Postfix(EquipSlotWeapon __instance)
        //{
        //    Logger.LogInfo("------------------------------------");
        //    Logger.LogInfo("EquipSlotWeaponOnAdd");
        //    Logger.LogInfo(__instance.OwnerId);
        //    Logger.LogInfo(__instance.ToString());
        //    Logger.LogInfo(__instance.GetType());
        //    Logger.LogInfo(__instance.m_pickableId);
            
        //}

        //[HarmonyPostfix, HarmonyPatch(typeof(EquipSlotWeapon), "OnRemove")]
        //private static void EquipSlotWeapon_OnRemove_Postfix(EquipSlotWeapon __instance)
        //{
        //    if (__instance.OwnerId != leftHandItemID && __instance.OwnerId != rightHandItemID)
        //    {
        //        return;
        //    }
        //    Logger.LogInfo("------------------------------------");
        //    Logger.LogInfo("EquipSlotWeaponOnRemove");
        //}


        //[HarmonyPostfix, HarmonyPatch(typeof(NetworkPlayer), "UserCode_CmdItemUse")]
        //private static void NetworkPlayer_UserCode_CmdItemUse_Postfix(NetworkPlayer __instance)
        //{
        //    if (__instance._isLocal || __instance.IsLocal)
        //    {
        //        Logger.LogInfo("------------------------------------");
        //        Logger.LogInfo("UserCode_CmdItemUse");
        //    }
        //}

        private static string leftEquipSlotPlayer = null;
        private static string rightEquipSlotPlayer = null;
        [HarmonyPrefix, HarmonyPatch(typeof(EquipSlotPlayer), "OnAdd")]
        private static void EquipSlotPlayer_OnAdd_Prefix(EquipSlotPlayer __instance)
        {
            if (__instance.OwnerId != localPlayerID)
            {
                return;
            }
            leftEquipSlotPlayer = __instance.LeftPickedObjId;
            rightEquipSlotPlayer = __instance.RightPickedObjId;
            Logger.LogInfo(__instance.LeftPickedObjId);
            Logger.LogInfo(__instance.RightPickedObjId);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(EquipSlotPlayer), "OnAdd")]
        private static void EquipSlotPlayer_OnAdd_Postfix(EquipSlotPlayer __instance)
        {
            if (__instance.OwnerId != localPlayerID)
            {
                return;
            }
            if (MainData.SettingSaveData.weaponSettingData.weaponMainHand == InputHand.RightHand)
            {
                if (__instance.LeftPickedObjId != null && leftEquipSlotPlayer == null)
                {
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo("ChestSlotOutputItem");
                    _TrueGear.Play("ChestSlotOutputItem");
                }
                else if (__instance.RightPickedObjId != null && rightEquipSlotPlayer == null)
                {
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo("RightHipSlotOutputItem");
                    _TrueGear.Play("RightHipSlotOutputItem");
                }
            }
            else
            {
                if (__instance.LeftPickedObjId != null && leftEquipSlotPlayer == null)
                {
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo("LeftHipSlotOutputItem");
                    _TrueGear.Play("LeftHipSlotOutputItem");
                }
                else if (__instance.RightPickedObjId != null && rightEquipSlotPlayer == null)
                {
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo("ChestSlotOutputItem");
                    _TrueGear.Play("ChestSlotOutputItem");
                }
            }
            leftEquipSlotPlayer = null;
            rightEquipSlotPlayer = null;
            Logger.LogInfo(__instance.LeftPickedObjId);
            Logger.LogInfo(__instance.RightPickedObjId);
            Logger.LogInfo(__instance.OwnerId);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(EquipSlotPlayer), "OnRemove")]
        private static void EquipSlotPlayer_OnRemove_Prefix(EquipSlotPlayer __instance)
        {
            if (__instance.OwnerId != localPlayerID)
            {
                return;
            }
            Logger.LogInfo(__instance.LeftPickedObjId);
            Logger.LogInfo(__instance.RightPickedObjId);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(EquipSlotPlayer), "OnRemove")]
        private static void EquipSlotPlayer_OnRemove_Postfix(EquipSlotPlayer __instance, string id)
        {
            if (__instance.OwnerId != localPlayerID)
            {
                return;
            }
            if (MainData.SettingSaveData.weaponSettingData.weaponMainHand == InputHand.RightHand)
            {
                if (__instance.LeftPickedObjId == id && id == gunID)
                {
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo("ChestSlotInputItem");
                    _TrueGear.Play("ChestSlotInputItem");
                }
                else if (__instance.RightPickedObjId == id && id == gunID)
                {
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo("RightHipSlotInputItem");
                    _TrueGear.Play("RightHipSlotInputItem");
                }
            }
            else
            {
                if (__instance.LeftPickedObjId == id && id == gunID)
                {
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo("LeftHipSlotInputItem");
                    _TrueGear.Play("LeftHipSlotInputItem");
                }
                else if (__instance.RightPickedObjId == id && id == gunID)
                {
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo("ChestSlotInputItem");
                    _TrueGear.Play("ChestSlotInputItem");
                }
            }
            Logger.LogInfo(id);
            Logger.LogInfo(__instance.LeftPickedObjId);
            Logger.LogInfo(__instance.RightPickedObjId);
            Logger.LogInfo(__instance.OwnerId);
            Logger.LogInfo(gunID);
        }


        //[HarmonyPrefix, HarmonyPatch(typeof(PacksackSlotChip), "OnAdd")]
        //private static void PacksackSlotChip_OnAdd_Postfix(PacksackSlotChip __instance, string id)
        //{
        //    Logger.LogInfo("------------------------------------");
        //    Logger.LogInfo("PacksackSlotChipOnAdd");
        //}

        //[HarmonyPrefix, HarmonyPatch(typeof(PacksackSlotChip), "OnRemove")]
        //private static void PacksackSlotChip_OnRemove_Postfix(PacksackSlotChip __instance, string id)
        //{
        //    Logger.LogInfo("------------------------------------");
        //    Logger.LogInfo("PacksackSlotChipOnRemove");
        //}




        //[HarmonyPrefix, HarmonyPatch(typeof(PlayerModelGameAgent), "Hited")]
        //private static void PlayerModelGameAgent_Hited_Postfix(PlayerModelGameAgent __instance, BuffTypeId buffTypeId, DamageResult damage)
        //{
        //    if (__instance.ID != localPlayerID)
        //    {
        //        return;
        //    }
        //    if (damage.UseData.HitPoint.x == 0 && damage.UseData.HitPoint.y == 0 && damage.UseData.HitPoint.z == 0)
        //    {
        //        Logger.LogInfo("------------------------------------");
        //        Logger.LogInfo($"PoisonDamage");
        //        return;
        //    }
        //    var angle = GetAngle(damage.UseData.StartPosition, damage.UseData.EndPosition, damage.UseData.HitPoint);
        //    Logger.LogInfo("------------------------------------");
        //    Logger.LogInfo($"Hited,{angle.Key},{angle.Value}");
        //    Logger.LogInfo($"StartPos :{damage.UseData.StartPosition.x},{damage.UseData.StartPosition.y},{damage.UseData.StartPosition.z}");
        //    Logger.LogInfo($"EndPos :{damage.UseData.EndPosition.x},{damage.UseData.EndPosition.y},{damage.UseData.EndPosition.z}");
        //    Logger.LogInfo($"HitPoint :{damage.UseData.HitPoint.x},{damage.UseData.HitPoint.y},{damage.UseData.HitPoint.z}");
        //    Logger.LogInfo($"WeaponId :{damage.WeaponId}");
        //    Logger.LogInfo($"AttackerId :{damage.AttackerId}");
        //    Logger.LogInfo($"BuffTypeId :{damage.BuffData.BuffTypeId}");

        //}

        //[HarmonyPrefix, HarmonyPatch(typeof(AliveLocalStatus), "OnExit")]
        //private static void AliveLocalStatus_OnExit_Postfix(AliveLocalStatus __instance)
        //{
        //    Logger.LogInfo("------------------------------------");
        //    Logger.LogInfo("OnExit");
        //    PlayerModelGameAgent playerModelGameAgent = Singleton<PlayerModelMgr>.Instance.Find<PlayerModelGameAgent>(localPlayerID);
        //    if (playerModelGameAgent != null)
        //    {
        //        playerModelGameAgent.OnHitedEvent.RemoveListener(new Action<BuffTypeId, DamageResult>(TrueGearHitedEvent));
        //    }

        //}

        //private static void TrueGearHitedEvent(BuffTypeId buffType, DamageResult damage)
        //{
        //    var angle = GetAngle(damage.UseData.StartPosition, damage.UseData.EndPosition, damage.UseData.HitPoint);
        //    Logger.LogInfo("------------------------------------");
        //    Logger.LogInfo($"TrueGearHitedEvent,{angle.Key},{angle.Value}");


        //    Logger.LogInfo($"StartPos :{damage.UseData.StartPosition.x},{damage.UseData.StartPosition.y},{damage.UseData.StartPosition.z}");
        //    Logger.LogInfo($"EndPos :{damage.UseData.EndPosition.x},{damage.UseData.EndPosition.y},{damage.UseData.EndPosition.z}");
        //    Logger.LogInfo($"HitPoint :{damage.UseData.HitPoint.x},{damage.UseData.HitPoint.y},{damage.UseData.HitPoint.z}");
        //    Logger.LogInfo($"WeaponId :{damage.WeaponId}");
        //    Logger.LogInfo($"AttackerId :{damage.AttackerId}");
        //    Logger.LogInfo($"BuffTypeId :{damage.BuffData.BuffTypeId}");
        //}

        private static bool isNormalDamage = false;

        [HarmonyPrefix, HarmonyPatch(typeof(PlayUI_PUBGHitPerformancePanel), "SetHitEffect",new Type[] { typeof(string) ,typeof(float) ,typeof(bool) })]
        private static void PlayUI_PUBGHitPerformancePanel_SetHitEffect_Postfix(PlayUI_PUBGHitPerformancePanel __instance,string attackerID, float hp, bool isHitShield)
        {
            if (__instance.playerOrigin.ID != localPlayerID)
            {
                return;
            }
            if (Singleton<GameModeMgr>.Instance.CurrentConfig.mode == GameMode.吃鸡模式)
            {
                PlayerModel playerModel = Singleton<PlayerModelMgr>.Instance.Find(attackerID);
                if (playerModel == null)
                {
                    PlayerAIModel playerAIModel = Singleton<PlayerAIModelMgr>.Instance.Find(attackerID);
                    if (playerAIModel != null)
                    {
                        var angle = GetAngle(__instance.playerOrigin.Head.transform, playerAIModel.transform.position);
                        Logger.LogInfo("------------------------------------");
                        Logger.LogInfo($"playerAIModelHit,{angle.Key},{angle.Value}");
                        _TrueGear.PlayAngle("DefaultDamage", angle.Key, angle.Value);
                    }
                    else
                    {
                        Logger.LogInfo("------------------------------------");
                        Logger.LogInfo($"playerNoooooAIModelHit");
                        _TrueGear.Play("PoisonDamage");
                    }
                }
                else if (playerModel != null)
                {
                    var angle = GetAngle(__instance.playerOrigin.Head.transform, playerModel.transform.position);
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo($"playerModelHit,{angle.Key},{angle.Value}");
                    _TrueGear.PlayAngle("DefaultDamage", angle.Key, angle.Value);
                }
                isNormalDamage = true;
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PlayUI_HitPerformancePanel), "SetHitEffect", new Type[] { typeof(string), typeof(float), typeof(bool) })]
        private static void PlayUI_HitPerformancePanel_SetHitEffect_Postfix(PlayUI_HitPerformancePanel __instance, string attackerID, float hp, bool isHitShield)
        {
            if (__instance.playerOrigin.ID != localPlayerID)
            {
                return;
            }
            if (Singleton<GameModeMgr>.Instance.CurrentConfig.mode == GameMode.冒险模式)
            {
                MonsterModel monsterModel = Singleton<MonsterModelMgr>.Instance.Find(attackerID);
                if (monsterModel != null)
                {
                    var angle = GetAngle(__instance.playerOrigin.Head.transform, monsterModel.transform.position);
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo($"MonsterModelHit,{angle.Key},{angle.Value}");
                    _TrueGear.PlayAngle("DefaultDamage", angle.Key, angle.Value);
                }
                else
                {
                    Logger.LogInfo("------------------------------------");
                    Logger.LogInfo($"NooooooMonsterModelHit");
                    _TrueGear.Play("PoisonDamage");
                }
                isNormalDamage = true;
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(RescueCount), "PlayRescue")]
        private static void RescueCount_PlayRescue_Postfix(RescueCount __instance, InputHand inputHand, string dyingID, RescueType type)
        {
            if (__instance.DyingID != localPlayerID)
            {
                return;
            }
            Logger.LogInfo("------------------------------------");
            Logger.LogInfo("StartPlayerRescue");
            _TrueGear.StartPlayerRescue();
            Logger.LogInfo(__instance.DyingID);
            Logger.LogInfo(localPlayerID);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(RescueCount), "StopRescue")]
        private static void RescueCount_StopRescue_Prefix(RescueCount __instance, InputHand inputHand, RescueType type)
        {
            if (__instance.DyingID != localPlayerID)
            {
                return;
            }
            Logger.LogInfo("------------------------------------");
            Logger.LogInfo("StopPlayerRescue");
            _TrueGear.StopPlayerRescue();
            Logger.LogInfo(__instance.DyingID);
            Logger.LogInfo(localPlayerID);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AliveLocalStatus_PUBG), "OnHited")]
        private static void AliveLocalStatus_PUBG_OnHited_Postfix(AliveLocalStatus_PUBG __instance, PlayUI_StatusEffectPanel2 _statusEffectPanel, BuffTypeId buffType, DamageResult damage)
        {
            if (_statusEffectPanel.origin.ID != localPlayerID)
            {
                return;
            }
            if (damage.BuffData.BuffDescribe == "毒圈伤害")
            {
                Logger.LogInfo("------------------------------------");
                Logger.LogInfo("PoisonDamage");
                _TrueGear.Play("PoisonDamage");
            }
            if (isNormalDamage)
            {
                isNormalDamage = false;
                return;
            }
            isNormalDamage = false;
            if (buffType == BuffTypeId.DistanceDecayDamge)
            {
                Logger.LogInfo("------------------------------------");
                Logger.LogInfo("Explosion");
                _TrueGear.Play("Explosion");
            }
        }

    }
}
