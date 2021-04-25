using System.ComponentModel;
using Terraria.Localization;
using Terraria.ModLoader.Config;

namespace LoRBGM
{
    public class LoRConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [DefaultValue(true)]
        [Label("播放音乐前报幕")]
        [Tooltip("Make a BGM claim before playing")]
        public bool BGMClaim;

        [DefaultValue(true)]
        [Label("在主菜单播放大厅音乐")]
        [Tooltip("Play lobby music in main menu")]
        public bool LobbyBGM;

        [DefaultValue(true)]
        [Label("结束战斗时播放结算音乐")]
        [Tooltip("Play victory music after a fighting")]
        public bool ResultBGM;

        [DefaultValue(true)]
        [Label("是否播放来宾音乐")]
        [Tooltip("Play Early Battle BGM")]
        public bool BossBGM;

        [DefaultValue(true)]
        [Label("是否播放下层楼层音乐")]
        [Tooltip("Play Asiyah floor BGM")]
        public bool BGMAsiyah;

        [DefaultValue(true)]
        [Label("是否播放中层楼层音乐")]
        [Tooltip("Play Briah floor BGM")]
        public bool BGMBriah;

        [DefaultValue(true)]
        [Label("是否播放上层楼层音乐（包括Kether）")]
        [Tooltip("Play Atziluth floor BGM (including Kether)")]
        public bool BGMAtziluth;

        [DefaultValue(true)]
        [Label("是否播放解放战的楼层音乐")]
        [Tooltip("Play Realize battle BGM")]
        public bool RealizeBGM;

        [DefaultValue(true)]
        [Label("是否播放残响乐团战的楼层音乐")]
        [Tooltip("Play Reverberation Ensemble BGM")]
        public bool ReverberationBGM;

        [DefaultValue(true)]
        [Label("是否播放扭曲残响乐团战的音乐")]
        [Tooltip("Play Reverberation Ensemble Distortion BGM")]
        public bool Reverberation2BGM;



        [Label("打开所有")]
        [Tooltip("Toggle All On")]
        public bool SetAllOn
        {
            get => false;
            set
            {
                if (value)
                {
                    LobbyBGM = true;
                    ResultBGM = true;
                    BGMAsiyah = true;
                    BGMAtziluth = true;
                    BGMBriah = true;
                    BossBGM = true;
                    RealizeBGM = true;
                    ReverberationBGM = true;
                    Reverberation2BGM = true;
                }
            }
        }


        [Label("关闭所有")]
        [Tooltip("Toggle All Off")]
        public bool SetAllOff
        {
            get => false;
            set
            {
                if (value)
                {
                    LobbyBGM = false;
                    ResultBGM = false;
                    BGMAsiyah = false;
                    BGMAtziluth = false;
                    BGMBriah = false;
                    BossBGM = false;
                    RealizeBGM = false;
                    ReverberationBGM = false;
                    Reverberation2BGM = false;
                }
            }
        }

        public override ModConfig Clone()
        {
            var clone = (LoRConfig)base.Clone();
            return clone;
        }

        public override void OnLoaded()
        {
            LoRBGM.LoRConfig = this;
        }


        public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string messageline)
        {
            string message = "";
            string messagech = "";

            if (Language.ActiveCulture == GameCulture.Chinese)
            {
                messageline = messagech;
            }
            else
            {
                messageline = message;
            }

            if (whoAmI == 0)
            {
                //message = "Changes accepted!";
                //messagech = "设置改动成功!";
                return true;
            }
            if (whoAmI != 0)
            {
                //message = "You have no rights to change config.";
                //messagech = "你没有设置改动权限.";
                return false;
            }
            return false;
        }
    }
}