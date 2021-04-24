
namespace DataWallServer
{
    partial class DataWallServer_Main
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.Workspace = new System.Windows.Forms.TabControl();
            this.UsersActivities = new System.Windows.Forms.TabPage();
            this.ActivitiesGrid = new System.Windows.Forms.DataGridView();
            this.ShowAll = new System.Windows.Forms.RadioButton();
            this.ShowInactive = new System.Windows.Forms.RadioButton();
            this.ShowActive = new System.Windows.Forms.RadioButton();
            this.UsersData = new System.Windows.Forms.TabPage();
            this.SoftDataGrid = new System.Windows.Forms.DataGridView();
            this.SelectUser = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.DeviceDataGrid = new System.Windows.Forms.DataGridView();
            this.ConsoleTab = new System.Windows.Forms.TabPage();
            this.ConsoleText = new System.Windows.Forms.TextBox();
            this.Redriver = new System.Windows.Forms.Timer(this.components);
            this.Workspace.SuspendLayout();
            this.UsersActivities.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ActivitiesGrid)).BeginInit();
            this.UsersData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SoftDataGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DeviceDataGrid)).BeginInit();
            this.ConsoleTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // Workspace
            // 
            this.Workspace.Controls.Add(this.UsersActivities);
            this.Workspace.Controls.Add(this.UsersData);
            this.Workspace.Controls.Add(this.ConsoleTab);
            this.Workspace.Location = new System.Drawing.Point(13, 13);
            this.Workspace.Name = "Workspace";
            this.Workspace.SelectedIndex = 0;
            this.Workspace.Size = new System.Drawing.Size(775, 425);
            this.Workspace.TabIndex = 0;
            // 
            // UsersActivities
            // 
            this.UsersActivities.Controls.Add(this.ActivitiesGrid);
            this.UsersActivities.Controls.Add(this.ShowAll);
            this.UsersActivities.Controls.Add(this.ShowInactive);
            this.UsersActivities.Controls.Add(this.ShowActive);
            this.UsersActivities.Location = new System.Drawing.Point(4, 22);
            this.UsersActivities.Name = "UsersActivities";
            this.UsersActivities.Padding = new System.Windows.Forms.Padding(3);
            this.UsersActivities.Size = new System.Drawing.Size(767, 399);
            this.UsersActivities.TabIndex = 0;
            this.UsersActivities.Text = "Users Activities";
            this.UsersActivities.UseVisualStyleBackColor = true;
            // 
            // ActivitiesGrid
            // 
            this.ActivitiesGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ActivitiesGrid.Location = new System.Drawing.Point(7, 30);
            this.ActivitiesGrid.Name = "ActivitiesGrid";
            this.ActivitiesGrid.Size = new System.Drawing.Size(754, 363);
            this.ActivitiesGrid.TabIndex = 3;
            this.ActivitiesGrid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.ActivitiesGrid_CellContentClick);
            // 
            // ShowAll
            // 
            this.ShowAll.AutoSize = true;
            this.ShowAll.Checked = true;
            this.ShowAll.Location = new System.Drawing.Point(196, 6);
            this.ShowAll.Name = "ShowAll";
            this.ShowAll.Size = new System.Drawing.Size(66, 17);
            this.ShowAll.TabIndex = 2;
            this.ShowAll.TabStop = true;
            this.ShowAll.Text = "Show All";
            this.ShowAll.UseVisualStyleBackColor = true;
            // 
            // ShowInactive
            // 
            this.ShowInactive.AutoSize = true;
            this.ShowInactive.Location = new System.Drawing.Point(97, 6);
            this.ShowInactive.Name = "ShowInactive";
            this.ShowInactive.Size = new System.Drawing.Size(93, 17);
            this.ShowInactive.TabIndex = 1;
            this.ShowInactive.Text = "Show Inactive";
            this.ShowInactive.UseVisualStyleBackColor = true;
            // 
            // ShowActive
            // 
            this.ShowActive.AutoSize = true;
            this.ShowActive.Location = new System.Drawing.Point(6, 6);
            this.ShowActive.Name = "ShowActive";
            this.ShowActive.Size = new System.Drawing.Size(85, 17);
            this.ShowActive.TabIndex = 0;
            this.ShowActive.Text = "Show Active";
            this.ShowActive.UseVisualStyleBackColor = true;
            // 
            // UsersData
            // 
            this.UsersData.Controls.Add(this.SoftDataGrid);
            this.UsersData.Controls.Add(this.SelectUser);
            this.UsersData.Controls.Add(this.comboBox1);
            this.UsersData.Controls.Add(this.DeviceDataGrid);
            this.UsersData.Location = new System.Drawing.Point(4, 22);
            this.UsersData.Name = "UsersData";
            this.UsersData.Padding = new System.Windows.Forms.Padding(3);
            this.UsersData.Size = new System.Drawing.Size(767, 399);
            this.UsersData.TabIndex = 1;
            this.UsersData.Text = "Users Data";
            this.UsersData.UseVisualStyleBackColor = true;
            // 
            // SoftDataGrid
            // 
            this.SoftDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.SoftDataGrid.Location = new System.Drawing.Point(385, 33);
            this.SoftDataGrid.Name = "SoftDataGrid";
            this.SoftDataGrid.Size = new System.Drawing.Size(376, 360);
            this.SoftDataGrid.TabIndex = 3;
            // 
            // SelectUser
            // 
            this.SelectUser.AutoSize = true;
            this.SelectUser.Location = new System.Drawing.Point(7, 9);
            this.SelectUser.Name = "SelectUser";
            this.SelectUser.Size = new System.Drawing.Size(62, 13);
            this.SelectUser.TabIndex = 2;
            this.SelectUser.Text = "Select User";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(75, 6);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(303, 21);
            this.comboBox1.TabIndex = 1;
            // 
            // DeviceDataGrid
            // 
            this.DeviceDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DeviceDataGrid.Location = new System.Drawing.Point(10, 33);
            this.DeviceDataGrid.Name = "DeviceDataGrid";
            this.DeviceDataGrid.Size = new System.Drawing.Size(368, 360);
            this.DeviceDataGrid.TabIndex = 0;
            // 
            // ConsoleTab
            // 
            this.ConsoleTab.Controls.Add(this.ConsoleText);
            this.ConsoleTab.Location = new System.Drawing.Point(4, 22);
            this.ConsoleTab.Name = "ConsoleTab";
            this.ConsoleTab.Padding = new System.Windows.Forms.Padding(3);
            this.ConsoleTab.Size = new System.Drawing.Size(767, 399);
            this.ConsoleTab.TabIndex = 2;
            this.ConsoleTab.Text = "Console";
            this.ConsoleTab.UseVisualStyleBackColor = true;
            // 
            // ConsoleText
            // 
            this.ConsoleText.Location = new System.Drawing.Point(7, 4);
            this.ConsoleText.Multiline = true;
            this.ConsoleText.Name = "ConsoleText";
            this.ConsoleText.ReadOnly = true;
            this.ConsoleText.Size = new System.Drawing.Size(754, 389);
            this.ConsoleText.TabIndex = 0;
            // 
            // Redriver
            // 
            this.Redriver.Enabled = true;
            this.Redriver.Interval = 500;
            this.Redriver.Tick += new System.EventHandler(this.Redriver_Tick);
            // 
            // DataWallServer_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.Workspace);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(816, 489);
            this.MinimumSize = new System.Drawing.Size(816, 489);
            this.Name = "DataWallServer_Main";
            this.Text = "DataWall Server";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DataWallServer_Main_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DataWallServer_Main_FormClosed);
            this.Workspace.ResumeLayout(false);
            this.UsersActivities.ResumeLayout(false);
            this.UsersActivities.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ActivitiesGrid)).EndInit();
            this.UsersData.ResumeLayout(false);
            this.UsersData.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SoftDataGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DeviceDataGrid)).EndInit();
            this.ConsoleTab.ResumeLayout(false);
            this.ConsoleTab.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl Workspace;
        private System.Windows.Forms.TabPage UsersActivities;
        private System.Windows.Forms.TabPage UsersData;
        private System.Windows.Forms.RadioButton ShowAll;
        private System.Windows.Forms.RadioButton ShowInactive;
        private System.Windows.Forms.RadioButton ShowActive;
        private System.Windows.Forms.DataGridView ActivitiesGrid;
        private System.Windows.Forms.DataGridView SoftDataGrid;
        private System.Windows.Forms.Label SelectUser;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.DataGridView DeviceDataGrid;
        private System.Windows.Forms.TabPage ConsoleTab;
        private System.Windows.Forms.TextBox ConsoleText;
        private System.Windows.Forms.Timer Redriver;
    }
}

