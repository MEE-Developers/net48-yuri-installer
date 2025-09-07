using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YuriInstaller.MizukiTools;
using YuriInstaller.Properties;

namespace YuriInstaller
{
    partial class Comment
    {
        /*
            这是安装事件，安装的步骤（复制文件、写入注册表）都在这里。
         */
    }

    partial class _StartWindow
    {
        /// <summary>闪烁窗口任务栏图标。</summary>
        [DllImport("User32.dll", CharSet = CharSet.Unicode, EntryPoint = "FlashWindow")]
        private static extern void FlashWindow(IntPtr hwnd, bool bInvert);

        private string registSoft = string.Empty;
        private CMDScript uninstScript = new CMDScript();

        private static Bytes freeSpace = 0;
        private static Bytes spaceNeeded = 0;
        private static Bytes currentSize = 0;

        private int randomint;

        // 每隔一段时间从画廊里选一张图片进去
        private void BgSwitchTimer_Tick(object sender, EventArgs e)
        {
            int newIndex;
            do
            {
                newIndex = MizukiTool.RandomObject.Next(Paints.Length);
            }
            while (randomint == newIndex);

            imageChange.Replay();
            randomint = newIndex;
            bgPaints.BackgroundImage = Paints[randomint];
            Application.DoEvents();
        }

        // 更新路径信息
        private void SetPathInfo()
        {
            freeSpace = -1;
            if (MizukiTool.IsGoodPath(pathBoxPanel.Text))
            {
                try
                {
                    var drive = new DriveInfo(Path.GetPathRoot(pathBoxPanel.Text));
                    if (drive.IsReady)
                    {
                        freeSpace = drive.AvailableFreeSpace;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
            else
            {
                Debug.WriteLine("Has invalid character.");
            }

            pathInfo.Backup(string.Format(
                Localization.PathPage, spaceNeeded,
                (int)spaceNeeded.ToKBytes(), (int)spaceNeeded.ToMBytes(), spaceNeeded.ToGBytes(),
                freeSpace, (int)freeSpace.ToKBytes(), (int)freeSpace.ToMBytes(), freeSpace.ToGBytes(), bottomButton4.Clicked ? string.Empty : Localization.LittleTip));
        }

        /// <summary>安装事件。</summary>
        private async Task InstallEvent(bool toEmptyFolder = false)
        {
            CurrentProgress++;
            await UpdateInterface();
            rightTopLabel.Text = $"{Localization.Installing}...\n";

            if (await TryToInstall())
            {
                SetInstallEndingPath();
                useableEndLabels = EndingLabels.Where(i => i.CanBeRun()).ToArray();
                schkReturnToWindows.Checks(string.IsNullOrEmpty(_GameLauncher));
                CurrentProgress++;
                FlashWindow(Handle, true);
            }
            else
            {
                bottomButton3.Enabled = true;
                progressBar1.Visible = false;

                CurrentProgress--;
                if (toEmptyFolder)
                {
                    Directory.Delete(pathBoxPanel.Text, true);
                }
            }

            Cursor = Program.ReleaseCursor;
            bottomButton4.Enabled = !bottomButton4.Clicked;
            rbtnMusicOnOff.Enabled = rbtnSoundOnOff.Enabled = lblCreditBtn.Enabled = true;
        }

        private string CreateGameShorts(string tip, string path, string filename, string del = null)
        {
            rightTopLabel.Text += $"{tip}...\n";
            if (!CreateShortcut(path, filename,
                                Path.Combine(GameUninstInfo.InstallPath, _GameLauncher),
                                _LaunchArguments))
            {
                rightTopLabel.Text += $"{Localization.Failed}...\n";
                return string.Empty;
            }

            string returns = Path.Combine(path, filename);
            del = del ?? returns;
            if (!string.IsNullOrWhiteSpace(del))
            {
                uninstScript.AddDeleteFolderCommand(del);
            }

            return returns;
        }

        /// <summary>尝试安装，并返回是否安装成功。</summary>
        private async Task<bool> TryToInstall()
        {
            randomint = MizukiTool.RandomObject.Next(Paints.Length);
            bgPaints.BackgroundImage = Paints[randomint];
            bgSwitchTimer.Start();
            bgPaints.Visible = true;
            Application.DoEvents();

            try
            {
                bottomButton1.Enabled = bottomButton2.Enabled =
                bottomButton3.Enabled = bottomButton4.Enabled =
                rbtnMusicOnOff.Enabled = rbtnSoundOnOff.Enabled = lblCreditBtn.Enabled = false;

                uninstScript.Clear();
                uninstScript.AddCommand("chcp 65001");
                uninstScript.AddCommand($"goto then\n\n:then\n echo \"{Localization.TipBeforeRun}\"");
                uninstScript.AddCommand("pause");
                uninstScript.AddCommand("cd /");

                GameUninstInfo.InstallPath = pathBoxPanel.Text;
                Cursor = Program.WaitingCursor;

                foreach (var i in SetupLbls)
                {
                    try
                    {
                        await ExtractAFile(i, GameUninstInfo.InstallPath);
                    }
                    catch (Exception ex)
                    {
                        MizukiTool.ExceptionWindow(ex, $"{string.Format(Localization.UnpackError, i.Filename)}\n{ex.Message}");
                        return false;
                    }
                }

                if (chkCreateDesktopBox.Checked)
                {
                    GameUninstInfo.DesktopShortcut = CreateGameShorts(Localization.CreatingShortcut,
                    Program.DesktopPath, $"{Localization.GameName}.lnk");
                }

                if (chkCreateStartMenuBox.Checked)
                {
                    string startMenuPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), StartMenuPanel.Text);
                    GameUninstInfo.StartMenuPath = startMenuPath;
                    CreateGameShorts(Localization.CreStarShortcut, startMenuPath, $"{Localization.GameName}.lnk", startMenuPath);
                }

                progressBar1.Done();
                return true;
            }
            catch (Exception ex)
            {
                MizukiTool.ExceptionWindow(ex);
                return false;
            }
            finally
            {
                bgSwitchTimer.Stop();
                bgPaints.Visible = false;
            }
        }
    }
}
