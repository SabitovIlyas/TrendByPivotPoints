using System.Diagnostics;
using System.Text;

namespace ProjectsManager
{
    public class MainForm : Form
    {
        private const int MaxBufferLength = 4_000_000;
        private const int TrimmedBufferLength = 3_000_000;
        private const string SolutionFileName = "TrendByPivotPoints.sln";

        private readonly string solutionDir;
        private readonly string msbuildPath;
        private readonly List<ConsoleProject> projects = new();

        private ListView listView;
        private Label descriptionLabel;
        private ComboBox configCombo;
        private ComboBox platformCombo;
        private ComboBox encodingCombo;
        private TextBox argsBox;
        private TextBox outputBox;
        private TextBox inputBox;
        private Button buildButton;
        private Button runButton;
        private Button stopButton;
        private Button clearButton;
        private Button folderButton;
        private Button sendButton;
        private ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.Timer refreshTimer;
        private bool switching;

        public MainForm()
        {
            solutionDir = FindSolutionDir();
            msbuildPath = FindMsBuild();
            CreateProjects();
            CreateUi();
            FillProjectList();
        }

        private ConsoleProject Selected =>
            listView.SelectedItems.Count > 0 ? (ConsoleProject)listView.SelectedItems[0].Tag : null;

        private void CreateProjects()
        {
            projects.Add(new ConsoleProject
            {
                Name = "Starter (бэктест)",
                CsprojPath = @"TrendByPivotPointsStarter\TrendByPivotPointsStarter.csproj",
                AssemblyName = "TrendByPivotPointsStarter",
                Description = "Бэктест торговой системы Дончиана на исторических данных вне TSLab. Файл с данными выбирается в диалоговом окне.",
                DefaultEncodingIndex = 1
            });
            projects.Add(new ConsoleProject
            {
                Name = "Optimizator",
                CsprojPath = @"TrendByPivotPointsOptimizator\TrendByPivotPointsOptimizator.csproj",
                AssemblyName = "TrendByPivotPointsOptimizator",
                Description = "Оптимизатор параметров стратегий: генетический алгоритм, форвард-анализ. Настройка и запуск — кнопка «Оптимизатор…». Для больших данных собирайте с платформой x64.",
                DefaultEncodingIndex = 1
            });
            projects.Add(new ConsoleProject
            {
                Name = "CorrelationCalculator",
                CsprojPath = @"CorrelationCalculator\CorrelationCalculator.csproj",
                AssemblyName = "CorrelationCalculator",
                Description = "Расчёт корреляции инструментов по историческим данным.",
                DefaultEncodingIndex = 1
            });
            projects.Add(new ConsoleProject
            {
                Name = "HistoricalDataHelper",
                CsprojPath = @"HistoricalDataPreparationHelper\HistoricalDataPreparationHelper.csproj",
                AssemblyName = "HistoricalDataPreparationHelper",
                Description = "Подготовка исторических данных: конвертер unix-timestamp в дату (для бэктеста биткоина).",
                DefaultEncodingIndex = 1
            });
            projects.Add(new ConsoleProject
            {
                Name = "LogPreparator",
                CsprojPath = @"LogReader\LogPreparator.csproj",
                AssemblyName = "LogReader",
                Description = "Чтение и подготовка логов торговой системы.",
                DefaultEncodingIndex = 1
            });
            projects.Add(new ConsoleProject
            {
                Name = "PreparatorDataForSpread",
                CsprojPath = @"TrendByPivotPointsPeparatorDataForSpread\TrendByPivotPointsPeparatorDataForSpread.csproj",
                AssemblyName = "TrendByPivotPointsPeparatorDataForSpread",
                Description = "Подготовка данных для спредовой торговли.",
                DefaultEncodingIndex = 1
            });
            projects.Add(new ConsoleProject
            {
                Name = "SabitovCapitalConsole",
                CsprojPath = @"SabitovKapitalConsole\SabitovCapitalConsole.csproj",
                AssemblyName = "SabitovCapitalConsole",
                Description = "Учёт капитала и портфелей (.NET 7). Интерактивное меню — команды вводятся в поле «Ввод» внизу.",
                IsSdkStyle = true,
                DefaultEncodingIndex = 0
            });

            foreach (var project in projects)
                project.EncodingIndex = project.DefaultEncodingIndex;
        }

        private void CreateUi()
        {
            Text = "Менеджер проектов TrendByPivotPoints";
            Width = 1150;
            Height = 780;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9f);

            listView = new ListView
            {
                Dock = DockStyle.Left,
                Width = 300,
                View = View.Details,
                FullRowSelect = true,
                MultiSelect = false,
                HideSelection = false
            };
            listView.Columns.Add("Проект", 185);
            listView.Columns.Add("Статус", 95);
            listView.SelectedIndexChanged += (s, e) => OnSelectionChanged();

            descriptionLabel = new Label { Dock = DockStyle.Fill, AutoEllipsis = true };

            var controlsFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Margin = new Padding(0) };
            configCombo = CreateCombo(controlsFlow, "Конфигурация:", new[] { "Debug", "Release" });
            platformCombo = CreateCombo(controlsFlow, "Платформа:", new[] { "AnyCPU", "x64" });
            encodingCombo = CreateCombo(controlsFlow, "Кодировка:", new[] { "UTF-8", "CP866 (OEM)", "Windows-1251" });
            encodingCombo.SelectedIndexChanged += (s, e) =>
            {
                if (!switching && Selected != null)
                    Selected.EncodingIndex = encodingCombo.SelectedIndex;
            };

            var buttonsFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Margin = new Padding(0) };
            buildButton = CreateButton(buttonsFlow, "Собрать", (s, e) => BuildSelected());
            runButton = CreateButton(buttonsFlow, "Запустить", (s, e) => RunSelected());
            stopButton = CreateButton(buttonsFlow, "Остановить", (s, e) => StopSelected());
            clearButton = CreateButton(buttonsFlow, "Очистить вывод", (s, e) => ClearSelected());
            folderButton = CreateButton(buttonsFlow, "Открыть папку", (s, e) => OpenSelectedFolder());
            CreateButton(buttonsFlow, "Оптимизатор…", (s, e) => OpenOptimizatorSettings());

            var argsPanel = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            var argsLabel = new Label { Text = "Аргументы:", AutoSize = true, Location = new Point(3, 7) };
            argsBox = new TextBox
            {
                Location = new Point(85, 3),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            argsBox.Width = argsPanel.Width - 95;
            argsBox.TextChanged += (s, e) =>
            {
                if (!switching && Selected != null)
                    Selected.Args = argsBox.Text;
            };
            argsPanel.Controls.Add(argsLabel);
            argsPanel.Controls.Add(argsBox);

            var topTable = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 120,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(6, 4, 6, 0)
            };
            topTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 22f));
            topTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
            topTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
            topTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
            topTable.Controls.Add(descriptionLabel, 0, 0);
            topTable.Controls.Add(controlsFlow, 0, 1);
            topTable.Controls.Add(argsPanel, 0, 2);
            topTable.Controls.Add(buttonsFlow, 0, 3);

            outputBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                WordWrap = false,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Consolas", 9.5f),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.Gainsboro,
                MaxLength = 0
            };

            var inputPanel = new Panel { Dock = DockStyle.Bottom, Height = 34, Padding = new Padding(6, 4, 6, 4) };
            var inputLabel = new Label { Text = "Ввод:", AutoSize = true, Location = new Point(6, 10) };
            sendButton = new Button
            {
                Text = "Отправить",
                Width = 90,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            inputBox = new TextBox
            {
                Location = new Point(55, 7),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            sendButton.Location = new Point(inputPanel.Width - sendButton.Width - 6, 6);
            inputBox.Width = sendButton.Left - inputBox.Left - 8;
            sendButton.Click += (s, e) => SendInput();
            inputBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    SendInput();
                }
            };
            inputPanel.Controls.Add(inputLabel);
            inputPanel.Controls.Add(inputBox);
            inputPanel.Controls.Add(sendButton);

            var rightPanel = new Panel { Dock = DockStyle.Fill };
            rightPanel.Controls.Add(outputBox);
            rightPanel.Controls.Add(topTable);
            rightPanel.Controls.Add(inputPanel);
            outputBox.BringToFront();

            var statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel
            {
                Text = $"Решение: {solutionDir}    MSBuild: {(msbuildPath ?? "не найден!")}"
            };
            statusStrip.Items.Add(statusLabel);

            Controls.Add(rightPanel);
            Controls.Add(listView);
            Controls.Add(statusStrip);
            rightPanel.BringToFront();

            refreshTimer = new System.Windows.Forms.Timer { Interval = 100 };
            refreshTimer.Tick += (s, e) => RefreshProjects();
            refreshTimer.Start();

            FormClosing += OnFormClosing;
        }

        private static ComboBox CreateCombo(FlowLayoutPanel parent, string title, string[] items)
        {
            parent.Controls.Add(new Label { Text = title, AutoSize = true, Margin = new Padding(3, 9, 0, 0) });
            var combo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 110,
                Margin = new Padding(3, 5, 10, 0)
            };
            combo.Items.AddRange(items);
            combo.SelectedIndex = 0;
            parent.Controls.Add(combo);
            return combo;
        }

        private static Button CreateButton(FlowLayoutPanel parent, string text, EventHandler onClick)
        {
            var button = new Button { Text = text, AutoSize = true, Margin = new Padding(3, 3, 6, 3) };
            button.Click += onClick;
            parent.Controls.Add(button);
            return button;
        }

        private void FillProjectList()
        {
            foreach (var project in projects)
            {
                var item = new ListViewItem(project.Name) { Tag = project };
                item.SubItems.Add("—");
                project.Item = item;
                listView.Items.Add(item);
            }
            if (listView.Items.Count > 0)
                listView.Items[0].Selected = true;
        }

        private void OnSelectionChanged()
        {
            var project = Selected;
            if (project == null)
                return;

            switching = true;
            descriptionLabel.Text = project.Description;
            argsBox.Text = project.Args;
            encodingCombo.SelectedIndex = project.EncodingIndex;
            lock (project.SyncRoot)
            {
                outputBox.Text = project.Output.ToString() + project.Pending.ToString();
            }
            outputBox.SelectionStart = outputBox.TextLength;
            outputBox.ScrollToCaret();
            switching = false;
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            var project = Selected;
            var busy = project != null && project.IsBusy;
            buildButton.Enabled = project != null && !busy;
            runButton.Enabled = project != null && !busy;
            stopButton.Enabled = busy;
            sendButton.Enabled = busy && !project.IsBuilding;
            inputBox.Enabled = sendButton.Enabled;
        }

        private void BuildSelected()
        {
            var project = Selected;
            if (project == null || project.IsBusy)
                return;

            var csprojFull = Path.Combine(solutionDir, project.CsprojPath);
            ProcessStartInfo psi;
            if (project.IsSdkStyle)
            {
                psi = new ProcessStartInfo { FileName = "dotnet", WorkingDirectory = solutionDir };
                psi.ArgumentList.Add("build");
                psi.ArgumentList.Add(csprojFull);
                psi.ArgumentList.Add("-c");
                psi.ArgumentList.Add(configCombo.Text);
                psi.ArgumentList.Add("-v");
                psi.ArgumentList.Add("m");
                psi.ArgumentList.Add("--nologo");
            }
            else
            {
                if (msbuildPath == null)
                {
                    AppendToProject(project, "MSBuild не найден. Установите Visual Studio или Build Tools.\r\n");
                    return;
                }
                psi = new ProcessStartInfo { FileName = msbuildPath, WorkingDirectory = solutionDir };
                psi.ArgumentList.Add(csprojFull);
                psi.ArgumentList.Add("-t:Restore,Build");
                psi.ArgumentList.Add("-p:RestorePackagesConfig=true");
                psi.ArgumentList.Add("-p:SolutionDir=" + solutionDir + "\\");
                psi.ArgumentList.Add("-p:Configuration=" + configCombo.Text);
                psi.ArgumentList.Add("-p:Platform=" + platformCombo.Text);
                psi.ArgumentList.Add("-v:m");
                psi.ArgumentList.Add("-nologo");
            }

            StartProcess(project, psi, isBuild: true,
                $"— Сборка {project.Name} ({configCombo.Text}|{platformCombo.Text}) —");
        }

        private void RunSelected()
        {
            var project = Selected;
            if (project == null || project.IsBusy)
                return;

            var exePath = FindExecutable(project);
            if (exePath == null)
            {
                AppendToProject(project,
                    "Исполняемый файл не найден. Сначала соберите проект (кнопка «Собрать»).\r\n");
                return;
            }

            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = project.Args,
                WorkingDirectory = Path.GetDirectoryName(exePath)
            };
            StartProcess(project, psi, isBuild: false, $"— Запуск: {exePath} {project.Args} —");
        }

        private void StartProcess(ConsoleProject project, ProcessStartInfo psi, bool isBuild, string header)
        {
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.RedirectStandardInput = true;
            var encoding = GetEncoding(project.EncodingIndex);
            psi.StandardOutputEncoding = encoding;
            psi.StandardErrorEncoding = encoding;
            psi.StandardInputEncoding = encoding;

            try
            {
                var process = Process.Start(psi);
                process.StandardInput.AutoFlush = true;
                project.Process = process;
                project.IsBuilding = isBuild;
                AppendToProject(project, header + "\r\n");
                project.StdoutTask = PumpAsync(project, process.StandardOutput);
                project.StderrTask = PumpAsync(project, process.StandardError);
            }
            catch (Exception ex)
            {
                AppendToProject(project, "Не удалось запустить процесс: " + ex.Message + "\r\n");
                project.Process = null;
            }
            UpdateProjectStatus(project);
            UpdateButtons();
        }

        private async Task PumpAsync(ConsoleProject project, StreamReader reader)
        {
            var buffer = new char[4096];
            try
            {
                while (true)
                {
                    var count = await reader.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                    if (count == 0)
                        break;
                    AppendToProject(project, new string(buffer, 0, count));
                }
            }
            catch
            {
            }
        }

        private void StopSelected()
        {
            var project = Selected;
            if (project == null)
                return;
            KillProject(project);
        }

        private static void KillProject(ConsoleProject project)
        {
            try
            {
                project.Process?.Kill(entireProcessTree: true);
            }
            catch
            {
            }
        }

        private void ClearSelected()
        {
            var project = Selected;
            if (project == null)
                return;
            lock (project.SyncRoot)
            {
                project.Output.Clear();
                project.Pending.Clear();
            }
            outputBox.Clear();
        }

        //Настройки оптимизатора: конфигурирование стратегии и генетического
        //алгоритма из GUI; «Сохранить и запустить» передаёт файл настроек
        //оптимизатору аргументом — диалоги ему не нужны.
        private void OpenOptimizatorSettings()
        {
            using var form = new OptimizatorSettingsForm();
            if (form.ShowDialog(this) != DialogResult.OK || !form.RunRequested)
                return;

            var project = projects.FirstOrDefault(p =>
                p.AssemblyName == "TrendByPivotPointsOptimizator");
            if (project == null)
                return;

            project.Item.Selected = true;
            if (project.IsBusy)
            {
                AppendToProject(project, "Оптимизатор уже запущен — остановите его перед новым запуском.\r\n");
                return;
            }

            project.Args = "\"" + form.SettingsFilePath + "\"";
            argsBox.Text = project.Args;
            RunSelected();
        }

        private void OpenSelectedFolder()
        {
            var project = Selected;
            if (project == null)
                return;
            var exePath = FindExecutable(project);
            var folder = exePath != null
                ? Path.GetDirectoryName(exePath)
                : Path.Combine(solutionDir, Path.GetDirectoryName(project.CsprojPath));
            try
            {
                Process.Start("explorer.exe", folder);
            }
            catch (Exception ex)
            {
                AppendToProject(project, "Не удалось открыть папку: " + ex.Message + "\r\n");
            }
        }

        private void SendInput()
        {
            var project = Selected;
            if (project == null || !project.IsBusy || project.IsBuilding)
                return;
            var text = inputBox.Text;
            try
            {
                project.Process.StandardInput.WriteLine(text);
                AppendToProject(project, "> " + text + "\r\n");
                inputBox.Clear();
            }
            catch (Exception ex)
            {
                AppendToProject(project, "Не удалось отправить ввод: " + ex.Message + "\r\n");
            }
        }

        private void AppendToProject(ConsoleProject project, string text)
        {
            lock (project.SyncRoot)
            {
                project.Pending.Append(text);
            }
        }

        private void RefreshProjects()
        {
            foreach (var project in projects)
            {
                string chunk = null;
                bool trimmed = false;
                lock (project.SyncRoot)
                {
                    if (project.Pending.Length > 0)
                    {
                        chunk = project.Pending.ToString()
                            .Replace("\r\n", "\n").Replace("\n", "\r\n");
                        project.Pending.Clear();
                        project.Output.Append(chunk);
                        if (project.Output.Length > MaxBufferLength)
                        {
                            project.Output.Remove(0, project.Output.Length - TrimmedBufferLength);
                            trimmed = true;
                        }
                    }
                }

                if (chunk != null && project == Selected)
                {
                    if (trimmed)
                    {
                        lock (project.SyncRoot)
                        {
                            outputBox.Text = project.Output.ToString();
                        }
                        outputBox.SelectionStart = outputBox.TextLength;
                        outputBox.ScrollToCaret();
                    }
                    else
                    {
                        outputBox.AppendText(chunk);
                    }
                }

                var process = project.Process;
                if (process != null && process.HasExited &&
                    project.StdoutTask?.IsCompleted != false &&
                    project.StderrTask?.IsCompleted != false &&
                    project.Pending.Length == 0)
                {
                    int exitCode;
                    try
                    {
                        exitCode = process.ExitCode;
                    }
                    catch
                    {
                        exitCode = -1;
                    }
                    project.Process = null;
                    project.StdoutTask = null;
                    project.StderrTask = null;
                    var what = project.IsBuilding ? "Сборка завершена" : "Процесс завершён";
                    project.IsBuilding = false;
                    AppendToProject(project, $"\r\n— {what}, код выхода: {exitCode} —\r\n\r\n");
                    UpdateProjectStatus(project, exitCode);
                    UpdateButtons();
                }
                else
                {
                    UpdateProjectStatus(project);
                }
            }
        }

        private void UpdateProjectStatus(ConsoleProject project, int? exitCode = null)
        {
            string status;
            if (project.IsBusy)
                status = project.IsBuilding ? "Сборка…" : "Работает";
            else if (exitCode.HasValue)
                status = "Код: " + exitCode.Value;
            else
                status = project.Item.SubItems[1].Text;

            if (project.Item.SubItems[1].Text != status)
                project.Item.SubItems[1].Text = status;
        }

        private string FindExecutable(ConsoleProject project)
        {
            var binDir = Path.Combine(solutionDir,
                Path.GetDirectoryName(project.CsprojPath), "bin");
            if (!Directory.Exists(binDir))
                return null;
            return Directory.GetFiles(binDir, project.AssemblyName + ".exe", SearchOption.AllDirectories)
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();
        }

        private static Encoding GetEncoding(int index)
        {
            switch (index)
            {
                case 1: return Encoding.GetEncoding(866);
                case 2: return Encoding.GetEncoding(1251);
                default: return new UTF8Encoding(false);
            }
        }

        private static string FindSolutionDir()
        {
            foreach (var start in new[] { AppContext.BaseDirectory, Environment.CurrentDirectory })
            {
                var dir = new DirectoryInfo(start);
                while (dir != null)
                {
                    if (File.Exists(Path.Combine(dir.FullName, SolutionFileName)))
                        return dir.FullName;
                    dir = dir.Parent;
                }
            }
            MessageBox.Show($"Не найден файл решения {SolutionFileName}. " +
                "Запустите менеджер из каталога решения.", "Менеджер проектов",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return Environment.CurrentDirectory;
        }

        private static string FindMsBuild()
        {
            var vswhere = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Microsoft Visual Studio", "Installer", "vswhere.exe");
            if (File.Exists(vswhere))
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = vswhere,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };
                    psi.ArgumentList.Add("-latest");
                    psi.ArgumentList.Add("-requires");
                    psi.ArgumentList.Add("Microsoft.Component.MSBuild");
                    psi.ArgumentList.Add("-find");
                    psi.ArgumentList.Add(@"MSBuild\**\Bin\MSBuild.exe");
                    using var process = Process.Start(psi);
                    var line = process.StandardOutput.ReadLine();
                    process.WaitForExit(10000);
                    if (!string.IsNullOrWhiteSpace(line) && File.Exists(line.Trim()))
                        return line.Trim();
                }
                catch
                {
                }
            }
            return null;
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            var running = projects.Where(p => p.IsBusy).ToList();
            if (running.Count == 0)
                return;

            var names = string.Join(", ", running.Select(p => p.Name));
            var answer = MessageBox.Show(
                $"Запущены процессы: {names}.\r\nЗавершить их и выйти?",
                "Менеджер проектов", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer == DialogResult.Yes)
            {
                foreach (var project in running)
                    KillProject(project);
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}
