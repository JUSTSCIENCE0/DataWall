
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
            this.login = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.passwd_hash = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.active = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.code = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ShowAll = new System.Windows.Forms.RadioButton();
            this.ShowInactive = new System.Windows.Forms.RadioButton();
            this.ShowActive = new System.Windows.Forms.RadioButton();
            this.UsersData = new System.Windows.Forms.TabPage();
            this.DeviceDataGrid = new System.Windows.Forms.DataGridView();
            this.id_computer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewCheckBoxColumn2 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.cpu = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.motherboard = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gpu = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SelectUser = new System.Windows.Forms.Label();
            this.UsersList = new System.Windows.Forms.ComboBox();
            this.SoftDataGrid = new System.Windows.Forms.DataGridView();
            this.id_software = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewCheckBoxColumn1 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ConsoleTab = new System.Windows.Forms.TabPage();
            this.ConsoleText = new System.Windows.Forms.TextBox();
            this.Redriver = new System.Windows.Forms.Timer(this.components);
            this.Workspace.SuspendLayout();
            this.UsersActivities.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ActivitiesGrid)).BeginInit();
            this.UsersData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DeviceDataGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SoftDataGrid)).BeginInit();
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
            this.ActivitiesGrid.AllowUserToAddRows = false;
            this.ActivitiesGrid.AllowUserToDeleteRows = false;
            this.ActivitiesGrid.AllowUserToResizeColumns = false;
            this.ActivitiesGrid.AllowUserToResizeRows = false;
            this.ActivitiesGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ActivitiesGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.login,
            this.passwd_hash,
            this.active,
            this.code});
            this.ActivitiesGrid.Location = new System.Drawing.Point(7, 30);
            this.ActivitiesGrid.Name = "ActivitiesGrid";
            this.ActivitiesGrid.Size = new System.Drawing.Size(754, 363);
            this.ActivitiesGrid.TabIndex = 3;
            // 
            // login
            // 
            this.login.Frozen = true;
            this.login.HeaderText = "login";
            this.login.Name = "login";
            this.login.ReadOnly = true;
            this.login.Width = 50;
            // 
            // passwd_hash
            // 
            this.passwd_hash.Frozen = true;
            this.passwd_hash.HeaderText = "passwd_hash";
            this.passwd_hash.Name = "passwd_hash";
            this.passwd_hash.ReadOnly = true;
            this.passwd_hash.Width = 500;
            // 
            // active
            // 
            this.active.Frozen = true;
            this.active.HeaderText = "active";
            this.active.Name = "active";
            this.active.ReadOnly = true;
            this.active.Width = 50;
            // 
            // code
            // 
            this.code.Frozen = true;
            this.code.HeaderText = "code";
            this.code.Name = "code";
            this.code.ReadOnly = true;
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
            this.ShowAll.CheckedChanged += new System.EventHandler(this.ShowAll_CheckedChanged);
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
            this.ShowInactive.CheckedChanged += new System.EventHandler(this.ShowInactive_CheckedChanged);
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
            this.ShowActive.CheckedChanged += new System.EventHandler(this.ShowActive_CheckedChanged);
            // 
            // UsersData
            // 
            this.UsersData.Controls.Add(this.DeviceDataGrid);
            this.UsersData.Controls.Add(this.SelectUser);
            this.UsersData.Controls.Add(this.UsersList);
            this.UsersData.Controls.Add(this.SoftDataGrid);
            this.UsersData.Location = new System.Drawing.Point(4, 22);
            this.UsersData.Name = "UsersData";
            this.UsersData.Padding = new System.Windows.Forms.Padding(3);
            this.UsersData.Size = new System.Drawing.Size(767, 399);
            this.UsersData.TabIndex = 1;
            this.UsersData.Text = "Users Data";
            this.UsersData.UseVisualStyleBackColor = true;
            // 
            // DeviceDataGrid
            // 
            this.DeviceDataGrid.AllowUserToAddRows = false;
            this.DeviceDataGrid.AllowUserToDeleteRows = false;
            this.DeviceDataGrid.AllowUserToResizeColumns = false;
            this.DeviceDataGrid.AllowUserToResizeRows = false;
            this.DeviceDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DeviceDataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.id_computer,
            this.dataGridViewCheckBoxColumn2,
            this.cpu,
            this.motherboard,
            this.gpu});
            this.DeviceDataGrid.Location = new System.Drawing.Point(385, 33);
            this.DeviceDataGrid.Name = "DeviceDataGrid";
            this.DeviceDataGrid.Size = new System.Drawing.Size(376, 360);
            this.DeviceDataGrid.TabIndex = 3;
            // 
            // id_computer
            // 
            this.id_computer.Frozen = true;
            this.id_computer.HeaderText = "id_computer";
            this.id_computer.Name = "id_computer";
            this.id_computer.ReadOnly = true;
            this.id_computer.Width = 50;
            // 
            // dataGridViewCheckBoxColumn2
            // 
            this.dataGridViewCheckBoxColumn2.Frozen = true;
            this.dataGridViewCheckBoxColumn2.HeaderText = "active";
            this.dataGridViewCheckBoxColumn2.Name = "dataGridViewCheckBoxColumn2";
            this.dataGridViewCheckBoxColumn2.ReadOnly = true;
            this.dataGridViewCheckBoxColumn2.Width = 50;
            // 
            // cpu
            // 
            this.cpu.Frozen = true;
            this.cpu.HeaderText = "cpu";
            this.cpu.Name = "cpu";
            this.cpu.ReadOnly = true;
            this.cpu.Width = 75;
            // 
            // motherboard
            // 
            this.motherboard.Frozen = true;
            this.motherboard.HeaderText = "motherboard";
            this.motherboard.Name = "motherboard";
            this.motherboard.ReadOnly = true;
            this.motherboard.Width = 75;
            // 
            // gpu
            // 
            this.gpu.Frozen = true;
            this.gpu.HeaderText = "gpu";
            this.gpu.Name = "gpu";
            this.gpu.ReadOnly = true;
            this.gpu.Width = 75;
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
            // UsersList
            // 
            this.UsersList.FormattingEnabled = true;
            this.UsersList.Location = new System.Drawing.Point(75, 6);
            this.UsersList.Name = "UsersList";
            this.UsersList.Size = new System.Drawing.Size(303, 21);
            this.UsersList.TabIndex = 1;
            this.UsersList.SelectedIndexChanged += new System.EventHandler(this.UsersList_SelectedIndexChanged);
            // 
            // SoftDataGrid
            // 
            this.SoftDataGrid.AllowUserToAddRows = false;
            this.SoftDataGrid.AllowUserToDeleteRows = false;
            this.SoftDataGrid.AllowUserToResizeColumns = false;
            this.SoftDataGrid.AllowUserToResizeRows = false;
            this.SoftDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.SoftDataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.id_software,
            this.dataGridViewCheckBoxColumn1,
            this.name,
            this.dataGridViewTextBoxColumn1});
            this.SoftDataGrid.Location = new System.Drawing.Point(10, 33);
            this.SoftDataGrid.Name = "SoftDataGrid";
            this.SoftDataGrid.Size = new System.Drawing.Size(368, 360);
            this.SoftDataGrid.TabIndex = 0;
            // 
            // id_software
            // 
            this.id_software.Frozen = true;
            this.id_software.HeaderText = "id_software";
            this.id_software.Name = "id_software";
            this.id_software.ReadOnly = true;
            this.id_software.Width = 70;
            // 
            // dataGridViewCheckBoxColumn1
            // 
            this.dataGridViewCheckBoxColumn1.Frozen = true;
            this.dataGridViewCheckBoxColumn1.HeaderText = "active";
            this.dataGridViewCheckBoxColumn1.Name = "dataGridViewCheckBoxColumn1";
            this.dataGridViewCheckBoxColumn1.ReadOnly = true;
            this.dataGridViewCheckBoxColumn1.Width = 50;
            // 
            // name
            // 
            this.name.Frozen = true;
            this.name.HeaderText = "name";
            this.name.Name = "name";
            this.name.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.Frozen = true;
            this.dataGridViewTextBoxColumn1.HeaderText = "code";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
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
            this.ConsoleText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ConsoleText.Size = new System.Drawing.Size(754, 389);
            this.ConsoleText.TabIndex = 0;
            // 
            // Redriver
            // 
            this.Redriver.Enabled = true;
            this.Redriver.Interval = 1000;
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
            ((System.ComponentModel.ISupportInitialize)(this.DeviceDataGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SoftDataGrid)).EndInit();
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
        private System.Windows.Forms.DataGridView DeviceDataGrid;
        private System.Windows.Forms.Label SelectUser;
        private System.Windows.Forms.ComboBox UsersList;
        private System.Windows.Forms.DataGridView SoftDataGrid;
        private System.Windows.Forms.TabPage ConsoleTab;
        private System.Windows.Forms.TextBox ConsoleText;
        private System.Windows.Forms.Timer Redriver;
        private System.Windows.Forms.DataGridViewTextBoxColumn login;
        private System.Windows.Forms.DataGridViewTextBoxColumn passwd_hash;
        private System.Windows.Forms.DataGridViewCheckBoxColumn active;
        private System.Windows.Forms.DataGridViewTextBoxColumn code;
        private System.Windows.Forms.DataGridViewTextBoxColumn id_software;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn name;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn id_computer;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn cpu;
        private System.Windows.Forms.DataGridViewTextBoxColumn motherboard;
        private System.Windows.Forms.DataGridViewTextBoxColumn gpu;
    }
}

