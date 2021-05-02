
namespace DataWallClient
{
    partial class ClientForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.AuthPanel = new System.Windows.Forms.Panel();
            this.Auth = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.password = new System.Windows.Forms.TextBox();
            this.Login = new System.Windows.Forms.TextBox();
            this.Workspace = new System.Windows.Forms.Panel();
            this.tabWorkspace = new System.Windows.Forms.TabControl();
            this.MyLibrary = new System.Windows.Forms.TabPage();
            this.Store = new System.Windows.Forms.TabPage();
            this.LibraryList = new System.Windows.Forms.ListBox();
            this.Download = new System.Windows.Forms.Button();
            this.Delete = new System.Windows.Forms.Button();
            this.Check = new System.Windows.Forms.Button();
            this.Repack = new System.Windows.Forms.Button();
            this.Run = new System.Windows.Forms.Button();
            this.StoreList = new System.Windows.Forms.ListBox();
            this.Add = new System.Windows.Forms.Button();
            this.AuthPanel.SuspendLayout();
            this.Workspace.SuspendLayout();
            this.tabWorkspace.SuspendLayout();
            this.MyLibrary.SuspendLayout();
            this.Store.SuspendLayout();
            this.SuspendLayout();
            // 
            // AuthPanel
            // 
            this.AuthPanel.Controls.Add(this.Auth);
            this.AuthPanel.Controls.Add(this.label2);
            this.AuthPanel.Controls.Add(this.label1);
            this.AuthPanel.Controls.Add(this.password);
            this.AuthPanel.Controls.Add(this.Login);
            this.AuthPanel.Location = new System.Drawing.Point(40, 120);
            this.AuthPanel.Name = "AuthPanel";
            this.AuthPanel.Size = new System.Drawing.Size(400, 200);
            this.AuthPanel.TabIndex = 0;
            // 
            // Auth
            // 
            this.Auth.Location = new System.Drawing.Point(275, 155);
            this.Auth.Name = "Auth";
            this.Auth.Size = new System.Drawing.Size(75, 23);
            this.Auth.TabIndex = 4;
            this.Auth.Text = "Вход";
            this.Auth.UseVisualStyleBackColor = true;
            this.Auth.Click += new System.EventHandler(this.Enter_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(50, 83);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "Пароль";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(50, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "Логин";
            // 
            // password
            // 
            this.password.Location = new System.Drawing.Point(50, 101);
            this.password.Name = "password";
            this.password.PasswordChar = '*';
            this.password.Size = new System.Drawing.Size(300, 23);
            this.password.TabIndex = 1;
            this.password.Text = "user";
            // 
            // Login
            // 
            this.Login.Location = new System.Drawing.Point(50, 45);
            this.Login.Name = "Login";
            this.Login.Size = new System.Drawing.Size(300, 23);
            this.Login.TabIndex = 0;
            this.Login.Text = "user";
            // 
            // Workspace
            // 
            this.Workspace.Controls.Add(this.tabWorkspace);
            this.Workspace.Location = new System.Drawing.Point(13, 13);
            this.Workspace.Name = "Workspace";
            this.Workspace.Size = new System.Drawing.Size(459, 436);
            this.Workspace.TabIndex = 1;
            this.Workspace.Visible = false;
            // 
            // tabWorkspace
            // 
            this.tabWorkspace.Controls.Add(this.MyLibrary);
            this.tabWorkspace.Controls.Add(this.Store);
            this.tabWorkspace.Location = new System.Drawing.Point(10, 13);
            this.tabWorkspace.Name = "tabWorkspace";
            this.tabWorkspace.SelectedIndex = 0;
            this.tabWorkspace.Size = new System.Drawing.Size(438, 410);
            this.tabWorkspace.TabIndex = 0;
            // 
            // MyLibrary
            // 
            this.MyLibrary.Controls.Add(this.Run);
            this.MyLibrary.Controls.Add(this.Repack);
            this.MyLibrary.Controls.Add(this.Check);
            this.MyLibrary.Controls.Add(this.Delete);
            this.MyLibrary.Controls.Add(this.Download);
            this.MyLibrary.Controls.Add(this.LibraryList);
            this.MyLibrary.Location = new System.Drawing.Point(4, 24);
            this.MyLibrary.Name = "MyLibrary";
            this.MyLibrary.Padding = new System.Windows.Forms.Padding(3);
            this.MyLibrary.Size = new System.Drawing.Size(430, 382);
            this.MyLibrary.TabIndex = 0;
            this.MyLibrary.Text = "Библиотека";
            this.MyLibrary.UseVisualStyleBackColor = true;
            // 
            // Store
            // 
            this.Store.Controls.Add(this.Add);
            this.Store.Controls.Add(this.StoreList);
            this.Store.Location = new System.Drawing.Point(4, 24);
            this.Store.Name = "Store";
            this.Store.Padding = new System.Windows.Forms.Padding(3);
            this.Store.Size = new System.Drawing.Size(430, 382);
            this.Store.TabIndex = 1;
            this.Store.Text = "Магазин";
            this.Store.UseVisualStyleBackColor = true;
            // 
            // LibraryList
            // 
            this.LibraryList.FormattingEnabled = true;
            this.LibraryList.ItemHeight = 15;
            this.LibraryList.Location = new System.Drawing.Point(4, 4);
            this.LibraryList.Name = "LibraryList";
            this.LibraryList.Size = new System.Drawing.Size(296, 364);
            this.LibraryList.TabIndex = 0;
            // 
            // Download
            // 
            this.Download.Enabled = false;
            this.Download.Location = new System.Drawing.Point(306, 4);
            this.Download.Name = "Download";
            this.Download.Size = new System.Drawing.Size(121, 41);
            this.Download.TabIndex = 1;
            this.Download.Text = "Загрузить";
            this.Download.UseVisualStyleBackColor = true;
            // 
            // Delete
            // 
            this.Delete.Enabled = false;
            this.Delete.Location = new System.Drawing.Point(306, 98);
            this.Delete.Name = "Delete";
            this.Delete.Size = new System.Drawing.Size(121, 41);
            this.Delete.TabIndex = 2;
            this.Delete.Text = "Удалить";
            this.Delete.UseVisualStyleBackColor = true;
            // 
            // Check
            // 
            this.Check.Enabled = false;
            this.Check.Location = new System.Drawing.Point(306, 145);
            this.Check.Name = "Check";
            this.Check.Size = new System.Drawing.Size(121, 41);
            this.Check.TabIndex = 3;
            this.Check.Text = "Проверить файлы";
            this.Check.UseVisualStyleBackColor = true;
            // 
            // Repack
            // 
            this.Repack.Enabled = false;
            this.Repack.Location = new System.Drawing.Point(306, 51);
            this.Repack.Name = "Repack";
            this.Repack.Size = new System.Drawing.Size(121, 41);
            this.Repack.TabIndex = 4;
            this.Repack.Text = "Переупаковать";
            this.Repack.UseVisualStyleBackColor = true;
            // 
            // Run
            // 
            this.Run.Enabled = false;
            this.Run.Location = new System.Drawing.Point(306, 327);
            this.Run.Name = "Run";
            this.Run.Size = new System.Drawing.Size(121, 41);
            this.Run.TabIndex = 5;
            this.Run.Text = "Запустить";
            this.Run.UseVisualStyleBackColor = true;
            // 
            // StoreList
            // 
            this.StoreList.FormattingEnabled = true;
            this.StoreList.ItemHeight = 15;
            this.StoreList.Location = new System.Drawing.Point(3, 3);
            this.StoreList.Name = "StoreList";
            this.StoreList.Size = new System.Drawing.Size(296, 364);
            this.StoreList.TabIndex = 1;
            // 
            // Add
            // 
            this.Add.Enabled = false;
            this.Add.Location = new System.Drawing.Point(305, 326);
            this.Add.Name = "Add";
            this.Add.Size = new System.Drawing.Size(121, 41);
            this.Add.TabIndex = 6;
            this.Add.Text = "Добавить в библиотеку";
            this.Add.UseVisualStyleBackColor = true;
            // 
            // ClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 461);
            this.Controls.Add(this.Workspace);
            this.Controls.Add(this.AuthPanel);
            this.MaximumSize = new System.Drawing.Size(500, 500);
            this.MinimumSize = new System.Drawing.Size(500, 500);
            this.Name = "ClientForm";
            this.Text = "Data Wall Client";
            this.AuthPanel.ResumeLayout(false);
            this.AuthPanel.PerformLayout();
            this.Workspace.ResumeLayout(false);
            this.tabWorkspace.ResumeLayout(false);
            this.MyLibrary.ResumeLayout(false);
            this.Store.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel AuthPanel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox password;
        private System.Windows.Forms.TextBox Login;
        private System.Windows.Forms.Button Auth;
        private System.Windows.Forms.Panel Workspace;
        private System.Windows.Forms.TabControl tabWorkspace;
        private System.Windows.Forms.TabPage MyLibrary;
        private System.Windows.Forms.TabPage Store;
        private System.Windows.Forms.ListBox LibraryList;
        private System.Windows.Forms.Button Check;
        private System.Windows.Forms.Button Delete;
        private System.Windows.Forms.Button Download;
        private System.Windows.Forms.Button Run;
        private System.Windows.Forms.Button Repack;
        private System.Windows.Forms.Button Add;
        private System.Windows.Forms.ListBox StoreList;
    }
}

