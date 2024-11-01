using System.Collections.Generic;
using System.Threading;
using System.IO;
using System;
using TrueGearSDK;
using System.Linq;



namespace MyTrueGear
{
    public class TrueGearMod
    {
        private static TrueGearPlayer _player = null;

        private static ManualResetEvent leftHandCourseRescueMRE = new ManualResetEvent(false);
        private static ManualResetEvent rightHandCourseRescueMRE = new ManualResetEvent(false);
        private static ManualResetEvent playerRescueMRE = new ManualResetEvent(false);
        private static ManualResetEvent leftHandSyringeMRE = new ManualResetEvent(false);
        private static ManualResetEvent rightHandSyringeMRE = new ManualResetEvent(false);
        private static ManualResetEvent leftHandBlastingToolPressMRE = new ManualResetEvent(false);
        private static ManualResetEvent rightHandBlastingToolPressMRE = new ManualResetEvent(false);

        public TrueGearMod()
        {
            _player = new TrueGearPlayer("2492600","Rampage Agents");
            _player.PreSeekEffect("DefaultDamage");
            _player.Start();
            new Thread(new ThreadStart(this.LeftHandCourseRescue)).Start();
            new Thread(new ThreadStart(this.RightHandCourseRescue)).Start();
            new Thread(new ThreadStart(this.PlayerRescue)).Start();
            new Thread(new ThreadStart(this.LeftHandSyringe)).Start();
            new Thread(new ThreadStart(this.RightHandSyringe)).Start();
            new Thread(new ThreadStart(this.LeftHandBlastingToolPress)).Start();
            new Thread(new ThreadStart(this.RightHandBlastingToolPress)).Start();
        }

        public void LeftHandCourseRescue()
        {
            while (true)
            {
                leftHandCourseRescueMRE.WaitOne();
                _player.SendPlay("LeftHandCourseRescue");
                Thread.Sleep(300);
            }            
        }

        public void RightHandCourseRescue()
        {
            while (true)
            {
                rightHandCourseRescueMRE.WaitOne();
                _player.SendPlay("RightHandCourseRescue");
                Thread.Sleep(300);
            }
        }

        public void PlayerRescue()
        {
            while (true)
            {
                playerRescueMRE.WaitOne();
                _player.SendPlay("PlayerRescue");
                Thread.Sleep(500);
            }
        }

        public void LeftHandSyringe()
        {
            while(true)
            {
                leftHandSyringeMRE.WaitOne();
                _player.SendPlay("LeftHandSyringe");
                Thread.Sleep(300);
            }
            
        }

        public void RightHandSyringe()
        {
            while(true)
            {
                rightHandSyringeMRE.WaitOne();
                _player.SendPlay("RightHandSyringe");
                Thread.Sleep(300);
            }            
        }

        public void LeftHandBlastingToolPress()
        {
            while (true)
            {
                leftHandBlastingToolPressMRE.WaitOne();
                _player.SendPlay("LeftHandBlastingToolPress");
                Thread.Sleep(130);
            }
            
        }

        public void RightHandBlastingToolPress()
        {
            while (true)
            {
                rightHandBlastingToolPressMRE.WaitOne();
                _player.SendPlay("RightHandBlastingToolPress");
                Thread.Sleep(130);
            }            
        }

        
        

        public void Play(string Event)
        { 
            _player.SendPlay(Event);
        }

        public void PlayAngle(string tmpEvent, float tmpAngle, float tmpVertical)
        {
            try
            {
                float angle = (tmpAngle - 22.5f) > 0f ? tmpAngle - 22.5f : 360f - tmpAngle;
                int horCount = (int)(angle / 45) + 1;

                int verCount = tmpVertical > 0.1f ? -4 : tmpVertical < 0f ? 8 : 0;

                TrueGearSDK.EffectObject oriObject = _player.FindEffectByUuid(tmpEvent);
                TrueGearSDK.EffectObject rootObject = TrueGearSDK.EffectObject.Copy(oriObject);

                foreach (TrackObject track in rootObject.trackList)
                {
                    if (track.action_type == ActionType.Shake)
                    {
                        for (int i = 0; i < track.index.Length; i++)
                        {
                            if (verCount != 0)
                            {
                                track.index[i] += verCount;
                            }
                            if (horCount < 8)
                            {
                                if (track.index[i] < 50)
                                {
                                    int remainder = track.index[i] % 4;
                                    if (horCount <= remainder)
                                    {
                                        track.index[i] = track.index[i] - horCount;
                                    }
                                    else if (horCount <= (remainder + 4))
                                    {
                                        var num1 = horCount - remainder;
                                        track.index[i] = track.index[i] - remainder + 99 + num1;
                                    }
                                    else
                                    {
                                        track.index[i] = track.index[i] + 2;
                                    }
                                }
                                else
                                {
                                    int remainder = 3 - (track.index[i] % 4);
                                    if (horCount <= remainder)
                                    {
                                        track.index[i] = track.index[i] + horCount;
                                    }
                                    else if (horCount <= (remainder + 4))
                                    {
                                        var num1 = horCount - remainder;
                                        track.index[i] = track.index[i] + remainder - 99 - num1;
                                    }
                                    else
                                    {
                                        track.index[i] = track.index[i] - 2;
                                    }
                                }
                            }
                        }
                        if (track.index != null)
                        {
                            track.index = track.index.Where(i => !(i < 0 || (i > 19 && i < 100) || i > 119)).ToArray();
                        }
                    }
                    else if (track.action_type == ActionType.Electrical)
                    {
                        for (int i = 0; i < track.index.Length; i++)
                        {
                            if (horCount <= 4)
                            {
                                track.index[i] = 0;
                            }
                            else
                            {
                                track.index[i] = 100;
                            }
                            if (horCount == 1 || horCount == 8 || horCount == 4 || horCount == 5)
                            {
                                track.index = new int[2] { 0, 100 };
                            }
                        }
                    }
                }
                _player.SendPlayEffectByContent(rootObject);
            }
            catch(Exception ex)
            { 
                Console.WriteLine("TrueGear Mod PlayAngle Error :" + ex.Message);
                _player.SendPlay(tmpEvent);
            }          
        }

        public void StartLeftHandCourseRescue()
        {
            leftHandCourseRescueMRE.Set();
        }

        public void StopLeftHandCourseRescue()
        {
            leftHandCourseRescueMRE.Reset();
        }

        public void StartRightHandCourseRescue()
        {
            rightHandCourseRescueMRE.Set();
        }

        public void StopRightHandCourseRescue()
        {
            rightHandCourseRescueMRE.Reset();
        }

        public void StartPlayerRescue()
        {
            playerRescueMRE.Set();
        }

        public void StopPlayerRescue()
        {
            playerRescueMRE.Reset();
        }

        public void StartLeftHandSyringe()
        {
            leftHandSyringeMRE.Set();
        }

        public void StartRightHandSyringe()
        {
            rightHandSyringeMRE.Set();
        }

        public void StopSyringe()
        {
            leftHandSyringeMRE.Reset();
            rightHandSyringeMRE.Reset();
        }

        public void StartLeftHandBlastingToolPress()
        {
            leftHandBlastingToolPressMRE.Set();
        }

        public void StartRightHandBlastingToolPress()
        {
            rightHandBlastingToolPressMRE.Set();
        }

        public void StopBlastingToolPress()
        {
            leftHandBlastingToolPressMRE.Reset();
            rightHandBlastingToolPressMRE.Reset();
        }



    }
}
