using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using YuriInstaller.MizukiTools;

namespace YuriInstaller
{
    partial class Comment
    {
        /*
            How to add installation options:
            (1) First declare a SetupLabel control in the class;
            (2) Add your SetupLabel to the setuplbls array;
            (3) Locate the section "// Installation options start" in this file and configure with reference to the example;
            (4) Register the variable for your installation option name in L10N.cs;
            (5) Define the content of the variable in the L10N method;
            (6) Locate the section "// Installation options start" in L10N.cs and configure with reference to the example;
            (7) Add your installation file in setup.spec. Remember that the second blank in each line must be "Temp";
            (8) Set the DisplayName of this SetupLabel instance to your customized name (if you want to);
            (9) For example, if your SetupLabel is bound to setup.7z, "Extracting setup.7z" will be displayed in the upper right corner. If you set DisplayName to "aaa", it will display "Extracting aaa";
            (10) Set the MD5 of the SetupLabel to verify the installation file.
        */

        /*
            How to add post-installation action options:
            (1) First declare an EndLabel control in the class;
            (2) Set RunPath to the program to be executed in the SetInstallEnding method;
            (3) Register the variable for your installation option name in L10N.cs;
            (4) Define the content of the variable in the L10N method;
            (5) Add your option to EndingLabels, then add the string variable defined earlier in the SetEndingSelectionText method.
        */
    }

    // 自定义标签、安装选项等
    partial class _StartWindow
    {
        // 定制编辑框面板 Custom TextPanels
        private static ScrollPBPanel contentPanel, licensePanel;
        private static MizukiTextBoxPanel pathBoxPanel, StartMenuPanel;
        private static MizukiTextBoxBase license;

        // 各种自定义标签实例 Custom Checkboxes
        private static readonly SetupCheckBox chkInstallSet = new SetupCheckBox();
        private static readonly SetupCheckBox chkInstallSetA = new SetupCheckBox();

        private static readonly MizukiCheckBox chkCreateDesktopBox = new MizukiCheckBox();
        private static readonly MizukiCheckBox chkCreateStartMenuBox = new MizukiCheckBox();
        private static readonly MizukiCheckBox chkJianRongXingBox = new MizukiCheckBox();
        private static readonly MizukiCheckBox chkAgreeLicenses = new MizukiCheckBox();

        private static readonly EndComboBox schkReturnToWindows = new EndComboBox();
        private static readonly EndComboBox schkRunGame = new EndComboBox();
        private static readonly EndComboBox schkReadmeFile = new EndComboBox();
        private static readonly EndComboBox schkSeeWWWeb = new EndComboBox();
        private static readonly EndComboBox schkSeeMyWeb = new EndComboBox();
        private static readonly EndComboBox schkOpenExplorer = new EndComboBox();
        private static readonly EndComboBox schkRegUI = new EndComboBox();

        /// <summary>初始化安装选项组</summary>
        private static SetupCheckBox[] SetupLbls { get; } = new SetupCheckBox[2]
        {
            chkInstallSet,
            chkInstallSetA
        };

        /// <summary>初始化普通自定义标签数组。</summary>
        private static MizukiCheckBoxBase[] MizukiLabels { get; set; } = new MizukiCheckBox[]
        {
            chkAgreeLicenses
        };

        /// <summary>初始化安装结束单选框组</summary>
        private static EndComboBox[] EndingLabels { get; } = new EndComboBox[]
        {
            schkRunGame,
            schkReadmeFile,
            schkReturnToWindows,
            schkSeeWWWeb,
            schkSeeMyWeb,
            // schkOpenExplorer,
            schkRegUI
        };

        /// <summary>安装附加任务选项。</summary>
        private static MizukiCheckBox[] installMissions = new MizukiCheckBox[3]
        {
            chkCreateDesktopBox,
            chkCreateStartMenuBox,
            chkJianRongXingBox
        };

        /// <summary>设置安装结束选项运行路径。</summary>
        private void SetInstallEndingPath()
        {
            string installPath = GameUninstInfo.InstallPath;
            schkReturnToWindows.RunPath = " ";
            schkRunGame.RunPath = Path.Combine(installPath, _GameLauncher);
            schkRunGame.Arguments = _LaunchArguments;
            schkReadmeFile.RunPath = Path.Combine(installPath, _ReadmeFile);
            schkSeeWWWeb.RunPath = _GameHomepage;
            schkSeeMyWeb.RunPath = _AuthorHomepage;
            schkOpenExplorer.RunPath = installPath;
            schkRegUI.RunPath = Path.Combine(installPath, _RegSetMD);
        }

        /// <summary>初始化各种自定义控件及其属性。</summary>
        private void CustomControls()
        {
            MizukiLabels = MizukiLabels.Concat(installMissions).Concat(SetupLbls).Concat(EndingLabels).ToArray();

            // 设置各标签的初始选中状态
            chkCreateDesktopBox.Checked = chkCreateStartMenuBox.Checked =
            chkInstallSet.Checked = schkRunGame.Checked = true;

            // 设置各标签的具体位置及事件处理（部分）
            chkAgreeLicenses.Location = new Point(67, 458);
            chkAgreeLicenses.Name = "chkAgreeLicenses";
            chkAgreeLicenses.CheckedChanged += (sender, e) => bottomButton2.Enabled = chkAgreeLicenses.Checked;

            chkCreateDesktopBox.Name = "chkCreateDesktopBox";
            chkCreateStartMenuBox.Name = "chkCreateStartMenuBox";
            chkJianRongXingBox.Name = "chkJianRongXingBox";

            schkRunGame.Name = "schkRunGame";
            schkReturnToWindows.Name = "schkReturnToWindows";
            schkReadmeFile.Name = "schkReadmeFile";
            schkSeeWWWeb.Name = "schkSeeWWWeb";
            schkSeeMyWeb.Name = "schkSeeMyWeb";
            schkOpenExplorer.Name = "schkOpenExplorer";
            schkRegUI.Name = "schkRegUI";

            SetSetupCheckbox();

            // 将所有自定义标签添加到控件集合中
            foreach (var chk in MizukiLabels)
            {
                Controls.Add(chk);
            }

            // 设置安装选项的位置
            SetSettingChkLocation(SetupLbls, new Point(67, 80), 25);
            SetSettingChkLocation(installMissions, new Point(67, 370), 25);
            CustomPanelsAndTextBoxes();

            chkCreateStartMenuBox.MouseClick += (sender, e) => StartMenuPanel.Visible = chkCreateStartMenuBox.Checked;
            chkCreateStartMenuBox.LocationChanged += ChkCreateStartMenuBox_Changed;
            chkCreateStartMenuBox.SizeChanged += ChkCreateStartMenuBox_Changed;
        }

        private void SetSetupCheckbox()
        {
            chkInstallSet.Name = "chkInstallSet";
            chkInstallSet.CheckDisabled = true;     // 必选项不能关闭
            chkInstallSet.Filename = "setup.7z";
            // chkInstallSet.MD5 = "25f242b57a007a3bd21e8b70256787d7";

            chkInstallSetA.Name = "chkInstallSetA";
            chkInstallSetA.Filename = "setup1.7z";
            // chkInstallSetA.MD5 = "b91989c54eca97cfb375009562860e4a";
        }

        /// <summary>初始化定制文本框面板。<br />
        /// Initialize custom paneles and textboxes.</summary>
        private void CustomPanelsAndTextBoxes()
        {
            contentPanel = new ScrollPBPanel();
            licensePanel = new ScrollPBPanel();
            pathBoxPanel = new MizukiTextBoxPanel();
            StartMenuPanel = new MizukiTextBoxPanel();

            contentPanel.SetChild(contentTree);
            licensePanel.SetChild(licenseSider);

            licenseSider.SizeChanged += (sender, e) => license.Size = licenseSider.Size;
            licenseSider.TextChanged += (sender, e) => license.Text = licenseSider.Text;

            license = new MizukiTextBoxBase
            {
                Location = MizukiTool.ZeroPoint,
                Multiline = true,
                Name = "license",
                ReadOnly = true,
                Size = new Size(100, 21)
            };

            Panel[] CustomPanels = new Panel[4]
            {
                contentPanel,
                StartMenuPanel,
                pathBoxPanel,
                licensePanel
            };

            CustomPanels.SuspendLayout();
            contentPanel.SetBasicProperties(new Point(64, 164), "contentPanel", new Size(502, 267), false);
            licensePanel.SetBasicProperties(new Point(66, 55), "licensePanel", new Size(495, 400));
            pathBoxPanel.SetBasicProperties(new Point(64, 330), "pathBoxPanel", new Size(502, 28), false);
            StartMenuPanel.SetBasicProperties(new Point(8, 576), "StartMenuPanel", new Size(100, 20), false);

            pathBoxPanel.Child.Name = "pathBox";
            pathBoxPanel.Child.Size = new Size(490, 17);
            pathBoxPanel.ReadOnly = true;
            pathBoxPanel.Child.MouseClick += new MouseEventHandler(PathBox_MouseClick);
            pathBoxPanel.Child.TextChanged += (sender, e) =>
            {
                SetPathInfo();
                pathInfo.ForeColor = ColorBoard.HutsuuTextColor;
            };

            StartMenuPanel.Child.Name = "StartMenuNameBox";
            StartMenuPanel.ImeMode = ImeMode.On;
            Controls.AddRange(CustomPanels);
            CustomPanels.ResumeLayout(false);
        }

        /// <summary>开始菜单文件夹编辑框相关。</summary>
        private void ChkCreateStartMenuBox_Changed(object sender, EventArgs e)
        {
            int targetH = StartMenuPanel.Child.Height + 4;
            StartMenuPanel.Location = new Point(chkCreateStartMenuBox.Right + 10, chkCreateStartMenuBox.Top + (chkCreateStartMenuBox.Height - targetH) / 2);
            StartMenuPanel.Size = new Size(pathBoxPanel.Right - StartMenuPanel.Left - (chkCreateStartMenuBox.Left - pathBoxPanel.Left), targetH);
        }

        private void PathBox_MouseClick(object sender, MouseEventArgs e)
        {
            bottomButton4.Clicked = true;
            BottomButton4_MouseClick(bottomButton4, e);
        }

        /// <summary>只有将用户协议拉到底才能同意协议。</summary>
        private void LicensePanel_ScrolledToBottom(object sender = null, EventArgs e = null)
        {
            licensePanel.ScrolledToBottom -= LicensePanel_ScrolledToBottom;
            chkAgreeLicenses.CheckDisabled = false;
            chkAgreeLicenses.Checked = chkAgreeLicenses.Checked;
            chkAgreeLicenses.BtmbarTip = null;
            chkAgreeLicenses.ForeColor = ColorBoard.HutsuuTextColor;
        }
    }
}
