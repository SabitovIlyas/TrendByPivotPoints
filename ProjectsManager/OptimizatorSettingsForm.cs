using System.Globalization;
using System.Text;
using System.Text.Json;

namespace ProjectsManager
{
    /// <summary>
    /// Форма настроек оптимизатора: стратегия, сторона, параметры генетического
    /// алгоритма, окна тестов, пути к данным и таблица диапазонов параметров.
    /// Сохраняет всё в текстовый файл настроек, который читает оптимизатор
    /// (строки «Ключ:Значение» и «Range:имя:мин:макс:шаг»).
    /// </summary>
    public class OptimizatorSettingsForm : Form
    {
        public string SettingsFilePath => settingsFileBox.Text.Trim();
        public bool RunRequested { get; private set; }

        private ComboBox strategyCombo;
        private ComboBox sideCombo;
        private ComboBox timeFrameCombo;
        private TextBox seedBox;
        private NumericUpDown populationBox;
        private NumericUpDown generationsBox;
        private NumericUpDown crossoverBox;
        private NumericUpDown mutationBox;
        private NumericUpDown patienceBox;
        private NumericUpDown backwardDaysBox;
        private NumericUpDown forwardDaysBox;
        private NumericUpDown forwardPeriodsBox;
        private NumericUpDown shiftWindowBox;
        private NumericUpDown equityBox;
        private NumericUpDown riskBox;
        private NumericUpDown tournamentBox;
        private NumericUpDown minDiversityBox;
        private CheckBox trimHistoryBox;
        private TextBox securitiesFileBox;
        private TextBox seedGenesFileBox;
        private TextBox logFileBox;
        private TextBox settingsFileBox;
        private DataGridView rangesGrid;
        private bool loadingUi;

        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TrendByPivotPoints", "ProjectsManager.json");

        public OptimizatorSettingsForm()
        {
            CreateUi();
            FillDefaultRanges();
            LoadLastSettingsFile();
        }

        private void CreateUi()
        {
            Text = "Настройки оптимизатора";
            Width = 820;
            Height = 840;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9f);
            MinimizeBox = false;

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 290,
                ColumnCount = 4,
                Padding = new Padding(8, 8, 8, 0)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22f));

            strategyCombo = AddCombo(table, "Стратегия:", StrategyCatalog.Strategies);
            sideCombo = AddCombo(table, "Сторона:", StrategyCatalog.Sides);
            timeFrameCombo = AddCombo(table, "Таймфрейм:", StrategyCatalog.TimeFrames);
            timeFrameCombo.SelectedIndex = 4;   //60min
            seedBox = AddTextBox(table, "Сид (пусто — случайно):");

            populationBox = AddNumeric(table, "Размер популяции:", 2, 100000, 100);
            generationsBox = AddNumeric(table, "Поколений:", 1, 100000, 300);
            crossoverBox = AddNumeric(table, "Вероятность кроссовера:", 0, 1, 0.85m, 2, 0.05m);
            mutationBox = AddNumeric(table, "Вероятность мутации:", 0, 1, 0.10m, 2, 0.05m);

            patienceBox = AddNumeric(table, "Терпение (поколений):", 1, 100000, 50);
            backwardDaysBox = AddNumeric(table, "Окно бэктеста, дней:", 1, 36500, 1460);
            forwardDaysBox = AddNumeric(table, "Окно форварда, дней:", 0, 36500, 1460);
            forwardPeriodsBox = AddNumeric(table, "Форвардных периодов:", 1, 1000, 10);

            shiftWindowBox = AddNumeric(table, "Смещение окна, дней:", 1, 36500, 30);
            equityBox = AddNumeric(table, "Стартовый капитал:", 1, 1000000000, 100000);
            riskBox = AddNumeric(table, "Риск на сделку, %:", 0.01m, 100, 2, 2, 0.5m);
            trimHistoryBox = AddCheckBox(table, "Обрезать лишнюю историю:", true);

            tournamentBox = AddNumeric(table, "Размер турнира:", 2, 1000, 4);
            minDiversityBox = AddNumeric(table, "Мин. разнообразие:", 0, 1, 0.10m, 2, 0.05m);

            strategyCombo.SelectedIndexChanged += (s, e) => OnStrategyOrSideChanged();
            sideCombo.SelectedIndexChanged += (s, e) => OnStrategyOrSideChanged();

            var filesTable = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 135,
                ColumnCount = 3,
                Padding = new Padding(8, 0, 8, 0)
            };
            filesTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200f));
            filesTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            filesTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40f));

            securitiesFileBox = AddFileRow(filesTable, "Описание инструментов (!Securities_*.txt):",
                () => BrowseOpen(securitiesFileBox, "Описание инструментов|!Securities*.txt|Текстовые файлы|*.txt|Все файлы|*.*"));
            seedGenesFileBox = AddFileRow(filesTable, "Затравочная хромосома (необязательно):",
                () => BrowseOpen(seedGenesFileBox, "JSON|*.json|Все файлы|*.*"));
            logFileBox = AddFileRow(filesTable, "Журнал прогона (необязательно):",
                () => BrowseSaveFile(logFileBox, "Журнал|*.log|Текстовые файлы|*.txt|Все файлы|*.*"));
            settingsFileBox = AddFileRow(filesTable, "Файл настроек (куда сохранить):",
                BrowseSettingsFile);

            rangesGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            rangesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Параметр",
                Name = "colName",
                ReadOnly = true,
                FillWeight = 40f
            });
            rangesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Мин",
                Name = "colMin",
                FillWeight = 20f
            });
            rangesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Макс",
                Name = "colMax",
                FillWeight = 20f
            });
            rangesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Шаг",
                Name = "colStep",
                FillWeight = 20f
            });

            var gridLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 22,
                Padding = new Padding(8, 4, 0, 0),
                Text = "Диапазоны поиска параметров стратегии (мин/макс/шаг можно править):"
            };

            var buttonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 44,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(8, 6, 8, 6)
            };
            var cancelButton = new Button { Text = "Отмена", Width = 100 };
            cancelButton.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            var saveButton = new Button { Text = "Сохранить", Width = 110 };
            saveButton.Click += (s, e) => SaveAndClose(run: false);
            var saveRunButton = new Button { Text = "Сохранить и запустить", Width = 170 };
            saveRunButton.Click += (s, e) => SaveAndClose(run: true);
            var loadButton = new Button { Text = "Загрузить из файла…", Width = 150 };
            loadButton.Click += (s, e) => LoadFromFileDialog();
            buttonsPanel.Controls.Add(cancelButton);
            buttonsPanel.Controls.Add(saveButton);
            buttonsPanel.Controls.Add(saveRunButton);
            buttonsPanel.Controls.Add(loadButton);

            Controls.Add(rangesGrid);
            Controls.Add(gridLabel);
            Controls.Add(filesTable);
            Controls.Add(table);
            Controls.Add(buttonsPanel);
            rangesGrid.BringToFront();
        }

        private ComboBox AddCombo(TableLayoutPanel table, string title, string[] items)
        {
            table.Controls.Add(new Label { Text = title, AutoSize = true, Margin = new Padding(3, 9, 0, 0) });
            var combo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill,
                Margin = new Padding(3, 5, 12, 0)
            };
            combo.Items.AddRange(items);
            combo.SelectedIndex = 0;
            table.Controls.Add(combo);
            return combo;
        }

        private TextBox AddTextBox(TableLayoutPanel table, string title)
        {
            table.Controls.Add(new Label { Text = title, AutoSize = true, Margin = new Padding(3, 9, 0, 0) });
            var box = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(3, 5, 12, 0) };
            table.Controls.Add(box);
            return box;
        }

        private NumericUpDown AddNumeric(TableLayoutPanel table, string title, decimal min,
            decimal max, decimal value, int decimals = 0, decimal increment = 1)
        {
            table.Controls.Add(new Label { Text = title, AutoSize = true, Margin = new Padding(3, 9, 0, 0) });
            var box = new NumericUpDown
            {
                Minimum = min,
                Maximum = max,
                Value = value,
                DecimalPlaces = decimals,
                Increment = increment,
                Dock = DockStyle.Fill,
                Margin = new Padding(3, 5, 12, 0)
            };
            table.Controls.Add(box);
            return box;
        }

        private CheckBox AddCheckBox(TableLayoutPanel table, string title, bool isChecked)
        {
            table.Controls.Add(new Label { Text = title, AutoSize = true, Margin = new Padding(3, 9, 0, 0) });
            var box = new CheckBox
            {
                Checked = isChecked,
                AutoSize = true,
                Margin = new Padding(3, 8, 12, 0)
            };
            table.Controls.Add(box);
            return box;
        }

        private TextBox AddFileRow(TableLayoutPanel table, string title, Action browse)
        {
            table.Controls.Add(new Label { Text = title, AutoSize = true, Margin = new Padding(3, 9, 0, 0) });
            var box = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(3, 5, 3, 0) };
            table.Controls.Add(box);
            var button = new Button { Text = "…", Width = 30, Margin = new Padding(0, 4, 3, 0) };
            button.Click += (s, e) => browse();
            table.Controls.Add(button);
            return box;
        }

        private void BrowseSaveFile(TextBox target, string filter)
        {
            using var dialog = new SaveFileDialog { Filter = filter, OverwritePrompt = false };
            if (target.Text.Trim().Length > 0)
                dialog.FileName = target.Text.Trim();
            if (dialog.ShowDialog(this) == DialogResult.OK)
                target.Text = dialog.FileName;
        }

        private void BrowseOpen(TextBox target, string filter)
        {
            using var dialog = new OpenFileDialog { Filter = filter };
            if (File.Exists(target.Text))
                dialog.InitialDirectory = Path.GetDirectoryName(target.Text);
            if (dialog.ShowDialog(this) == DialogResult.OK)
                target.Text = dialog.FileName;
        }

        private void BrowseSettingsFile()
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "Текстовые файлы|*.txt|Все файлы|*.*",
                OverwritePrompt = false,
                FileName = Path.GetFileName(settingsFileBox.Text)
            };
            if (settingsFileBox.Text.Length > 0)
            {
                var dir = Path.GetDirectoryName(settingsFileBox.Text);
                if (Directory.Exists(dir))
                    dialog.InitialDirectory = dir;
            }
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                settingsFileBox.Text = dialog.FileName;
                if (File.Exists(dialog.FileName))
                    LoadFromFile(dialog.FileName);
            }
        }

        private void OnStrategyOrSideChanged()
        {
            if (!loadingUi)
                FillDefaultRanges();
        }

        private void FillDefaultRanges()
        {
            rangesGrid.Rows.Clear();
            var parameters = StrategyCatalog.GetDefaultParameters(strategyCombo.Text, sideCombo.Text);
            foreach (var parameter in parameters)
                rangesGrid.Rows.Add(parameter.Name, Format(parameter.Min),
                    Format(parameter.Max), Format(parameter.Step));
        }

        private static string Format(double value) =>
            value.ToString(CultureInfo.InvariantCulture);

        private static double ParseDouble(string value) =>
            double.Parse(value.Trim().Replace(',', '.'), CultureInfo.InvariantCulture);

        //Логический флаг настроек пишется как «1»/«0», но читаем и «true»/«false».
        private static bool IsTrue(string value)
        {
            var trimmed = value.Trim();
            return trimmed == "1" ||
                trimmed.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        //-------------------- Сохранение --------------------

        private void SaveAndClose(bool run)
        {
            try
            {
                if (SettingsFilePath.Length == 0)
                {
                    MessageBox.Show(this, "Укажите файл настроек (куда сохранить).",
                        Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (run && !File.Exists(securitiesFileBox.Text.Trim()))
                {
                    MessageBox.Show(this, "Для запуска укажите существующий файл с инструментами.",
                        Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                File.WriteAllText(SettingsFilePath, ComposeSettingsText(), new UTF8Encoding(false));
                SaveLastSettingsFile();
                RunRequested = run;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Не удалось сохранить настройки: " + ex.Message,
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ComposeSettingsText()
        {
            var builder = new StringBuilder();
            builder.AppendLine("PositionSide:" + sideCombo.Text);
            builder.AppendLine("TimeFrames:" + timeFrameCombo.Text);
            builder.AppendLine("Strategy:" + strategyCombo.Text);

            if (int.TryParse(seedBox.Text.Trim(), out var seed))
                builder.AppendLine("Seed:" + seed);

            builder.AppendLine("PopulationSize:" + populationBox.Value);
            builder.AppendLine("Generations:" + generationsBox.Value);
            builder.AppendLine("CrossoverRate:" + crossoverBox.Value.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("MutationRate:" + mutationBox.Value.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("Patience:" + patienceBox.Value);
            builder.AppendLine("TournamentSize:" + tournamentBox.Value);
            builder.AppendLine("MinDiversity:" + minDiversityBox.Value.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("BackwardDays:" + backwardDaysBox.Value);
            builder.AppendLine("ForwardDays:" + forwardDaysBox.Value);
            builder.AppendLine("ForwardPeriodsCount:" + forwardPeriodsBox.Value);
            builder.AppendLine("ShiftWindowDays:" + shiftWindowBox.Value);
            builder.AppendLine("TrimHistory:" + (trimHistoryBox.Checked ? "1" : "0"));
            builder.AppendLine("Equity:" + equityBox.Value.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("RiskValuePrcnt:" + riskBox.Value.ToString(CultureInfo.InvariantCulture));

            if (securitiesFileBox.Text.Trim().Length > 0)
                builder.AppendLine("SecuritiesFile:" + securitiesFileBox.Text.Trim());
            if (seedGenesFileBox.Text.Trim().Length > 0)
                builder.AppendLine("SeedGenesFile:" + seedGenesFileBox.Text.Trim());
            if (logFileBox.Text.Trim().Length > 0)
                builder.AppendLine("LogFile:" + logFileBox.Text.Trim());

            foreach (DataGridViewRow row in rangesGrid.Rows)
            {
                var name = row.Cells["colName"].Value?.ToString();
                if (string.IsNullOrEmpty(name))
                    continue;
                var min = ParseDouble(row.Cells["colMin"].Value?.ToString() ?? "0");
                var max = ParseDouble(row.Cells["colMax"].Value?.ToString() ?? "0");
                var step = ParseDouble(row.Cells["colStep"].Value?.ToString() ?? "1");
                builder.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "Range:{0}:{1}:{2}:{3}", name, min, max, step));
            }

            return builder.ToString();
        }

        //-------------------- Загрузка --------------------

        private void LoadFromFileDialog()
        {
            using var dialog = new OpenFileDialog { Filter = "Текстовые файлы|*.txt|Все файлы|*.*" };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                settingsFileBox.Text = dialog.FileName;
                LoadFromFile(dialog.FileName);
            }
        }

        private void LoadFromFile(string fileName)
        {
            try
            {
                loadingUi = true;
                var ranges = new Dictionary<string, (double Min, double Max, double Step)>();

                foreach (var line in File.ReadAllLines(fileName))
                {
                    var separatorIndex = line.IndexOf(':');
                    if (separatorIndex <= 0)
                        continue;
                    var key = line.Substring(0, separatorIndex).Trim();
                    var value = line.Substring(separatorIndex + 1).Trim();

                    switch (key)
                    {
                        case "PositionSide": SelectItem(sideCombo, value); break;
                        case "TimeFrames": SelectItem(timeFrameCombo, value); break;
                        case "Strategy": SelectItem(strategyCombo, value); break;
                        case "Seed": seedBox.Text = value; break;
                        case "PopulationSize": SetValue(populationBox, value); break;
                        case "Generations": SetValue(generationsBox, value); break;
                        case "CrossoverRate": SetValue(crossoverBox, value); break;
                        case "MutationRate": SetValue(mutationBox, value); break;
                        case "Patience": SetValue(patienceBox, value); break;
                        case "TournamentSize": SetValue(tournamentBox, value); break;
                        case "MinDiversity": SetValue(minDiversityBox, value); break;
                        case "BackwardDays": SetValue(backwardDaysBox, value); break;
                        case "ForwardDays": SetValue(forwardDaysBox, value); break;
                        case "ForwardPeriodsCount": SetValue(forwardPeriodsBox, value); break;
                        case "ShiftWindowDays": SetValue(shiftWindowBox, value); break;
                        case "TrimHistory": trimHistoryBox.Checked = IsTrue(value); break;
                        case "Equity": SetValue(equityBox, value); break;
                        case "RiskValuePrcnt": SetValue(riskBox, value); break;
                        case "SecuritiesFile": securitiesFileBox.Text = value; break;
                        case "SeedGenesFile": seedGenesFileBox.Text = value; break;
                        case "LogFile": logFileBox.Text = value; break;
                        case "Range":
                            var parts = value.Split(':');
                            if (parts.Length == 4)
                                ranges[parts[0].Trim()] = (ParseDouble(parts[1]),
                                    ParseDouble(parts[2]), ParseDouble(parts[3]));
                            break;
                    }
                }

                FillDefaultRanges();
                foreach (DataGridViewRow row in rangesGrid.Rows)
                {
                    var name = row.Cells["colName"].Value?.ToString();
                    if (name != null && ranges.TryGetValue(name, out var range))
                    {
                        row.Cells["colMin"].Value = Format(range.Min);
                        row.Cells["colMax"].Value = Format(range.Max);
                        row.Cells["colStep"].Value = Format(range.Step);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Не удалось загрузить настройки: " + ex.Message,
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                loadingUi = false;
            }
        }

        private static void SelectItem(ComboBox combo, string value)
        {
            var index = combo.Items.IndexOf(value);
            if (index >= 0)
                combo.SelectedIndex = index;
        }

        private static void SetValue(NumericUpDown box, string value)
        {
            try
            {
                var parsed = (decimal)ParseDouble(value);
                if (parsed < box.Minimum) parsed = box.Minimum;
                if (parsed > box.Maximum) parsed = box.Maximum;
                box.Value = parsed;
            }
            catch (FormatException)
            {
            }
        }

        //-------------------- Память последнего файла --------------------

        private sealed class ManagerConfig
        {
            public string LastOptimizatorSettingsFile { get; set; } = string.Empty;
        }

        private void LoadLastSettingsFile()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                    return;
                var config = JsonSerializer.Deserialize<ManagerConfig>(File.ReadAllText(ConfigPath));
                if (config != null && File.Exists(config.LastOptimizatorSettingsFile))
                {
                    settingsFileBox.Text = config.LastOptimizatorSettingsFile;
                    LoadFromFile(config.LastOptimizatorSettingsFile);
                }
            }
            catch
            {
            }
        }

        private void SaveLastSettingsFile()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));
                var config = new ManagerConfig { LastOptimizatorSettingsFile = SettingsFilePath };
                File.WriteAllText(ConfigPath, JsonSerializer.Serialize(config,
                    new JsonSerializerOptions { WriteIndented = true }));
            }
            catch
            {
            }
        }
    }
}
