using System.ComponentModel;
using System.Windows.Forms;

namespace Cranium.Lib.Test.Tests.Reinforcement.Pong
{
    partial class Visualizer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            this.LPaddle = new System.Windows.Forms.Button();
            this.RPaddle = new System.Windows.Forms.Button();
            this.Ball = new System.Windows.Forms.RadioButton();
            this.ResetButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LPaddle
            // 
            this.LPaddle.BackColor = System.Drawing.Color.Red;
            this.LPaddle.Location = new System.Drawing.Point(639, 234);
            this.LPaddle.Name = "LPaddle";
            this.LPaddle.Size = new System.Drawing.Size(75, 122);
            this.LPaddle.TabIndex = 0;
            this.LPaddle.UseVisualStyleBackColor = false;
            // 
            // RPaddle
            // 
            this.RPaddle.BackColor = System.Drawing.Color.Lime;
            this.RPaddle.Location = new System.Drawing.Point(801, 403);
            this.RPaddle.Name = "RPaddle";
            this.RPaddle.Size = new System.Drawing.Size(75, 122);
            this.RPaddle.TabIndex = 1;
            this.RPaddle.UseVisualStyleBackColor = false;
            // 
            // Ball
            // 
            this.Ball.AutoSize = true;
            this.Ball.Location = new System.Drawing.Point(817, 267);
            this.Ball.Name = "Ball";
            this.Ball.Size = new System.Drawing.Size(21, 20);
            this.Ball.TabIndex = 2;
            this.Ball.TabStop = true;
            this.Ball.UseVisualStyleBackColor = true;
            // 
            // ResetButton
            // 
            this.ResetButton.Location = new System.Drawing.Point(776, 12);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Size = new System.Drawing.Size(75, 30);
            this.ResetButton.TabIndex = 3;
            this.ResetButton.Text = "Reset";
            this.ResetButton.UseVisualStyleBackColor = true;
            this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // Visualizer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(1676, 929);
            this.Controls.Add(this.ResetButton);
            this.Controls.Add(this.Ball);
            this.Controls.Add(this.RPaddle);
            this.Controls.Add(this.LPaddle);
            this.DoubleBuffered = true;
            this.Name = "Visualizer";
            this.Text = "Visualizer";
            this.Load += new System.EventHandler(this.Visualizer_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public Button LPaddle;
        public Button RPaddle;
        public RadioButton Ball;
        private Button ResetButton;
    }
}