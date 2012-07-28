﻿namespace MpeInstaller.Controls
{
  partial class ExtensionControlHost
  {
    /// <summary> 
    /// Erforderliche Designervariable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Vom Komponenten-Designer generierter Code

    /// <summary> 
    /// Erforderliche Methode für die Designerunterstützung. 
    /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
    /// </summary>
    private void InitializeComponent()
    {
      this.extensionControlCollapsed = new MpeInstaller.Controls.ExtensionControlCollapsed();
      this.extensionControlExpanded = new MpeInstaller.Controls.ExtensionControlExpanded();
      this.SuspendLayout();
      // 
      // extensionControlCollapsed
      // 
      this.extensionControlCollapsed.Dock = System.Windows.Forms.DockStyle.Fill;
      this.extensionControlCollapsed.Location = new System.Drawing.Point(0, 0);
      this.extensionControlCollapsed.Margin = new System.Windows.Forms.Padding(1);
      this.extensionControlCollapsed.Name = "extensionControlCollapsed";
      this.extensionControlCollapsed.Padding = new System.Windows.Forms.Padding(1);
      this.extensionControlCollapsed.Size = new System.Drawing.Size(550, 23);
      this.extensionControlCollapsed.TabIndex = 0;
      // 
      // extensionControlExpanded
      // 
      this.extensionControlExpanded.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
      this.extensionControlExpanded.Dock = System.Windows.Forms.DockStyle.Fill;
      this.extensionControlExpanded.ForeColor = System.Drawing.SystemColors.ButtonFace;
      this.extensionControlExpanded.Location = new System.Drawing.Point(0, 0);
      this.extensionControlExpanded.Margin = new System.Windows.Forms.Padding(1);
      this.extensionControlExpanded.Name = "extensionControlExpanded";
      this.extensionControlExpanded.Size = new System.Drawing.Size(550, 23);
      this.extensionControlExpanded.TabIndex = 0;
      // 
      // ExtensionControlHost
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
      this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Controls.Add(this.extensionControlCollapsed);
      this.Controls.Add(this.extensionControlExpanded);
      this.Margin = new System.Windows.Forms.Padding(1);
      this.Name = "ExtensionControlHost";
      this.Size = new System.Drawing.Size(550, 23);
      this.ResumeLayout(false);

    }

    #endregion

    private ExtensionControlExpanded extensionControlExpanded;
    private ExtensionControlCollapsed extensionControlCollapsed;
  }
}
