namespace EscapeRoomController
{
    partial class fmrSetClueDialog
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
            this.txtSetClue = new System.Windows.Forms.TextBox();
            this.btnSetClueCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtSetClue
            // 
            this.txtSetClue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSetClue.Location = new System.Drawing.Point(12, 12);
            this.txtSetClue.Multiline = true;
            this.txtSetClue.Name = "txtSetClue";
            this.txtSetClue.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtSetClue.Size = new System.Drawing.Size(460, 53);
            this.txtSetClue.TabIndex = 0;
            // 
            // btnSetClueCancel
            // 
            this.btnSetClueCancel.Location = new System.Drawing.Point(478, 12);
            this.btnSetClueCancel.Name = "btnSetClueCancel";
            this.btnSetClueCancel.Size = new System.Drawing.Size(26, 23);
            this.btnSetClueCancel.TabIndex = 2;
            this.btnSetClueCancel.Text = "X";
            this.btnSetClueCancel.UseVisualStyleBackColor = true;
            this.btnSetClueCancel.Click += new System.EventHandler(this.btnSetClueCancel_Click);
            // 
            // fmrSetClueDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(508, 78);
            this.Controls.Add(this.btnSetClueCancel);
            this.Controls.Add(this.txtSetClue);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "fmrSetClueDialog";
            this.Text = "Set Clue";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtSetClue;
        private System.Windows.Forms.Button btnSetClueCancel;
    }
}