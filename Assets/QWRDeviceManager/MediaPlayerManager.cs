using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QWR
{
    public partial class QWRDeviceManager
    {
        public class MediaPlayerManager
        {
            // playMode EAC link https://blog.google/products/google-ar-vr/bringing-pixels-front-and-center-vr-video/
            public enum VideoPlayMode
            {
                Mode_2D=0,
                Mode_3DLR=1,
                Mode_3DRL=2,
                Mode_3DTB=4,
                Mode_3DBT=8,
                Mode_Pano_2D=16,
                Mode_Pano_3DLR=17,
                Mode_Pano_3DRL=18,
                Mode_Pano_3DTB=20,
                Mode_Pano_3DBT=24,
                Mode_180_2D=32,
                Mode_180_3DLR=33,
                Mode_180_3DRL=34,
                Mode_180_3DTB=36,
                Mode_180_3DBT=40,
                Mode_Fisheye_2D=64,
                Mode_Fisheye_3DLR=65,
                Mode_Fisheye_3DRL=66,
                Mode_Fisheye_3DTB=68,
                Mode_Fisheye_3DBT=72,
                Mode_EAC=128
            }

            /// <summary>
            /// Play a video
            /// </summary>
            /// <param name="videoPath">video full path</param>
            /// <param name="playMode">play mode,type <see cref="播放模式"/></param>
            /// <param name="time">play mode,type <see cref="播放進度"/></param>
            /// <param name="pkgname">return to pkgname after exit player, goto launcher if not set</param>
            public static void PlayVideo(string videoPath, VideoPlayMode playMode,int time = 0, string pkgname="")
            {
                GetDeviceManager().Call("playVideo", videoPath,(int)playMode,time, pkgname);
            }

            /// <summary>
            /// 只用左眼播放
            /// </summary>
            public static void PlayWithLeftEye()
            {
                GetDeviceManager().Call("playWithLeftEye");
            }

            /// <summary>
            /// 只用右眼播放
            /// </summary>
            public static void PlayWithRightEye()
            {
                GetDeviceManager().Call("playWithRightEye");
            }

            /// <summary>
            /// 恢復雙眼播放
            /// </summary>
            public static void PlayWithTwoEye()
            {
                GetDeviceManager().Call("playWithTwoEye");
            }

            /// <summary>
            /// Pause or resume player
            /// </summary>
            /// <param name="pauseOrNot">"pause" -> to pause video
            ///                          "play"  -> resume play
            /// </param>
            public static void PauseVideo(string pauseOrNot)
            {
                GetDeviceManager().Call("pauseVideo", pauseOrNot);
            }

            /// <summary>
            /// Stop play
            /// </summary>
            public static void StopPlay()
            {
                GetDeviceManager().Call("stopPlay");
            }

            /// <summary>
            /// Change play mode
            /// </summary>
            /// <param name="videoPlayMode"></param>
            public static void SwitchPlayMode(VideoPlayMode videoPlayMode)
            {
                GetDeviceManager().Call("switchPlayMode", (int)videoPlayMode);
            }

            /// <summary>
            /// Seek player
            /// </summary>
            /// <param name="seekTime_s">seconds</param>
            public static void SeekPlayer(string seekTime_s)
            {
                GetDeviceManager().Call("seekPlayer", seekTime_s);
            }

            /// <summary>
            /// Mute Player
            /// </summary>
            /// <param name="mute">"mute" -> to mute
            ///                    "unmute" -> to release mute
            ///                    </param>
            public static void MutePlayer(string mute)
            {
                GetDeviceManager().Call("mutePlayer", mute);
            }

            /// <summary>
            /// Set volume level
            /// </summary>
            /// <param name="volumeValue">volume range [0-100]</param>
            public static void SetVolume(int volumeValue)
            {
                GetDeviceManager().Call("setVolume", volumeValue);
            }

            /// <summary>
            /// Freeze screen
            /// </summary>
            /// <param name="freeze">1 -> to freeze
            ///                      0 -> unfreeze </param>
            public static void FreezeScreen(int freeze)
            {
                GetDeviceManager().Call("freezeScreen", freeze);
            }

            /// <summary>
            /// Block touch
            /// </summary>
            /// <param name="block">1 -> to block touch panel
            ///                     0 -> unblock touch panel  </param>
            public static void BlockTouch(int block)
            {
                GetDeviceManager().Call("blockTouch", block);
            }

            /// <summary>
            /// prompt msg
            /// </summary>
            /// <param name="msg">message</param>
            public static void PopNotification(string msg)
            {
                GetDeviceManager().Call("popNotification", msg);
            }

            /// <summary>
            /// Set previous activity
            /// </summary>
            /// <param name="pkgname">preActivity package name, intent to the package after play end</param>
            public static void SetCurrentPackageName(string pkgname)
            {
                GetDeviceManager().Call("setCurrentPkgname", pkgname);
            }

            public static void RegisterMediaPlayerStatusCallback(MediaPlayerStatusHandler playerStatusHandler)
            {
                GetDeviceManager().Call("registerMediaPlayerStateCallback", new MediaStatusCallback(playerStatusHandler));
            }

            public delegate void MediaPlayerStatusHandler(string filename, string state);
            class MediaStatusCallback : AndroidJavaProxy
            {
                private MediaPlayerStatusHandler handler;
                public MediaStatusCallback(MediaPlayerStatusHandler handler)
                    : base("com.qwr.DeviceManager.PluginCallback$MediaPlayerCallback")
                {
                    if (handler != null)
                        this.handler = handler;
                }

                public void OnMediaPlayerState(string filename, string status)
                {
                    if (handler != null)
                        handler(filename, status);
                }
            }
        }
    }
}
