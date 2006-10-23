namespace SetupTv.Sections
{
  partial class XmlSetup
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
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
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.buttonBrowse = new System.Windows.Forms.Button();
      this.label13 = new System.Windows.Forms.Label();
      this.textBoxFolder = new System.Windows.Forms.TextBox();
      this.checkBox1 = new System.Windows.Forms.CheckBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.textBoxHours = new System.Windows.Forms.TextBox();
      this.textBoxMinutes = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // buttonBrowse
      // 
      this.buttonBrowse.Location = new System.Drawing.Point(360, 46);
      this.buttonBrowse.Name = "buttonBrowse";
      this.buttonBrowse.Size = new System.Drawing.Size(23, 23);
      this.buttonBrowse.TabIndex = 7;
      this.buttonBrowse.Text = "...";
      this.buttonBrowse.UseVisualStyleBackColor = true;
      this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
      // 
      // label13
      // 
      this.label13.AutoSize = true;
      this.label13.Location = new System.Drawing.Point(21, 24);
      this.label13.Name = "label13";
      this.label13.Size = new System.Drawing.Size(156, 13);
      this.label13.TabIndex = 6;
      this.label13.Text = "Folder where .xmltv file is stored";
      // 
      // textBoxFolder
      // 
      this.textBoxFolder.Location = new System.Drawing.Point(70, 49);
      this.textBoxFolder.Name = "textBoxFolder";
      this.textBoxFolder.Size = new System.Drawing.Size(284, 20);
      this.textBoxFolder.TabIndex = 5;
      this.textBoxFolder.TextChanged += new System.EventHandler(this.textBoxFolder_TextChanged);
      // 
      // checkBox1
      // 
      this.checkBox1.AutoSize = true;
      this.checkBox1.Location = new System.Drawing.Point(24, 92);
      this.checkBox1.Name = "checkBox1";
      this.checkBox1.Size = new System.Drawing.Size(259, 17);
      this.checkBox1.TabIndex = 8;
      this.checkBox1.Text = "Apply timezone compensation when loading xmltv";
      this.checkBox1.UseVisualStyleBackColor = true;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(47, 121);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(38, 13);
      this.label1.TabIndex = 9;
      this.label1.Text = "Hours:";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(172, 121);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(47, 13);
      this.label2.TabIndex = 10;
      this.label2.Text = "Minutes:";
      // 
      // textBoxHours
      // 
      this.textBoxHours.Location = new System.Drawing.Point(91, 118);
      this.textBoxHours.Name = "textBoxHours";
      this.textBoxHours.Size = new System.Drawing.Size(50, 20);
      this.textBoxHours.TabIndex = 11;
      // 
      // textBoxMinutes
      // 
      this.textBoxMinutes.Location = new System.Drawing.Point(225, 118);
      this.textBoxMinutes.Name = "textBoxMinutes";
      this.textBoxMinutes.Size = new System.Drawing.Size(50, 20);
      this.textBoxMinutes.TabIndex = 12;
      this.textBoxMinutes.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
      // 
      // XmlSetup
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.textBoxMinutes);
      this.Controls.Add(this.textBoxHours);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.checkBox1);
      this.Controls.Add(this.buttonBrowse);
      this.Controls.Add(this.label13);
      this.Controls.Add(this.textBoxFolder);
      this.Name = "XmlSetup";
      this.Size = new System.Drawing.Size(467, 388);
      this.Load += new System.EventHandler(this.XmlSetup_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button buttonBrowse;
    private System.Windows.Forms.Label label13;
    private System.Windows.Forms.TextBox textBoxFolder;
    private System.Windows.Forms.CheckBox checkBox1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox textBoxHours;
    private System.Windows.Forms.TextBox textBoxMinutes;

  }
}