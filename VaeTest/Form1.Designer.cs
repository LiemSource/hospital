namespace VaeTest
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.txtJsessid = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtParams = new System.Windows.Forms.RichTextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.txtSignRsult = new System.Windows.Forms.RichTextBox();
            this.txtResponse = new System.Windows.Forms.RichTextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.btnStop = new System.Windows.Forms.Button();
            this.numericRequestInterval = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.numericInterval = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.numericTaskCount = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btnStopFloor = new System.Windows.Forms.Button();
            this.labelReviewResult = new System.Windows.Forms.Label();
            this.txtFloors = new System.Windows.Forms.RichTextBox();
            this.btnStartFloor = new System.Windows.Forms.Button();
            this.lblCurrent = new System.Windows.Forms.Label();
            this.txtTargetFloor = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericRequestInterval)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericInterval)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericTaskCount)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 26);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "会话Id:";
            // 
            // txtJsessid
            // 
            this.txtJsessid.Location = new System.Drawing.Point(85, 21);
            this.txtJsessid.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtJsessid.Name = "txtJsessid";
            this.txtJsessid.Size = new System.Drawing.Size(449, 25);
            this.txtJsessid.TabIndex = 1;
            this.txtJsessid.TextChanged += new System.EventHandler(this.txtJsessid_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 72);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "参数:";
            // 
            // txtParams
            // 
            this.txtParams.Location = new System.Drawing.Point(85, 69);
            this.txtParams.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtParams.Name = "txtParams";
            this.txtParams.Size = new System.Drawing.Size(1429, 119);
            this.txtParams.TabIndex = 3;
            this.txtParams.Text = "";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.txtSignRsult);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.txtResponse);
            this.splitContainer1.Size = new System.Drawing.Size(1636, 500);
            this.splitContainer1.SplitterDistance = 284;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 4;
            // 
            // txtSignRsult
            // 
            this.txtSignRsult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSignRsult.Location = new System.Drawing.Point(0, 0);
            this.txtSignRsult.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtSignRsult.Name = "txtSignRsult";
            this.txtSignRsult.Size = new System.Drawing.Size(284, 500);
            this.txtSignRsult.TabIndex = 0;
            this.txtSignRsult.Text = "";
            // 
            // txtResponse
            // 
            this.txtResponse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtResponse.Location = new System.Drawing.Point(0, 0);
            this.txtResponse.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtResponse.Name = "txtResponse";
            this.txtResponse.Size = new System.Drawing.Size(1347, 500);
            this.txtResponse.TabIndex = 0;
            this.txtResponse.Text = "";
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(1416, 21);
            this.btnSend.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(100, 26);
            this.btnSend.TabIndex = 5;
            this.btnSend.Text = "开始";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(4, 4);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.btnStop);
            this.splitContainer2.Panel1.Controls.Add(this.numericRequestInterval);
            this.splitContainer2.Panel1.Controls.Add(this.label5);
            this.splitContainer2.Panel1.Controls.Add(this.numericInterval);
            this.splitContainer2.Panel1.Controls.Add(this.label4);
            this.splitContainer2.Panel1.Controls.Add(this.numericTaskCount);
            this.splitContainer2.Panel1.Controls.Add(this.label3);
            this.splitContainer2.Panel1.Controls.Add(this.label2);
            this.splitContainer2.Panel1.Controls.Add(this.btnSend);
            this.splitContainer2.Panel1.Controls.Add(this.label1);
            this.splitContainer2.Panel1.Controls.Add(this.txtJsessid);
            this.splitContainer2.Panel1.Controls.Add(this.txtParams);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer1);
            this.splitContainer2.Size = new System.Drawing.Size(1636, 662);
            this.splitContainer2.SplitterDistance = 157;
            this.splitContainer2.SplitterWidth = 5;
            this.splitContainer2.TabIndex = 6;
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(1537, 20);
            this.btnStop.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(100, 29);
            this.btnStop.TabIndex = 12;
            this.btnStop.Text = "停止";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // numericRequestInterval
            // 
            this.numericRequestInterval.Location = new System.Drawing.Point(1248, 21);
            this.numericRequestInterval.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.numericRequestInterval.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numericRequestInterval.Name = "numericRequestInterval";
            this.numericRequestInterval.Size = new System.Drawing.Size(160, 25);
            this.numericRequestInterval.TabIndex = 11;
            this.numericRequestInterval.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(1081, 26);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(151, 15);
            this.label5.TabIndex = 10;
            this.label5.Text = "发起请求间隔(毫秒):";
            // 
            // numericInterval
            // 
            this.numericInterval.Location = new System.Drawing.Point(912, 21);
            this.numericInterval.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.numericInterval.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numericInterval.Name = "numericInterval";
            this.numericInterval.Size = new System.Drawing.Size(160, 25);
            this.numericInterval.TabIndex = 9;
            this.numericInterval.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(783, 26);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(121, 15);
            this.label4.TabIndex = 8;
            this.label4.Text = "线程间隔(毫秒):";
            // 
            // numericTaskCount
            // 
            this.numericTaskCount.Location = new System.Drawing.Point(615, 21);
            this.numericTaskCount.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.numericTaskCount.Name = "numericTaskCount";
            this.numericTaskCount.Size = new System.Drawing.Size(160, 25);
            this.numericTaskCount.TabIndex = 7;
            this.numericTaskCount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(544, 26);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 15);
            this.label3.TabIndex = 6;
            this.label3.Text = "线程数:";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1652, 699);
            this.tabControl1.TabIndex = 7;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.splitContainer2);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage1.Size = new System.Drawing.Size(1644, 670);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Sign";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.btnStopFloor);
            this.tabPage2.Controls.Add(this.labelReviewResult);
            this.tabPage2.Controls.Add(this.txtFloors);
            this.tabPage2.Controls.Add(this.btnStartFloor);
            this.tabPage2.Controls.Add(this.lblCurrent);
            this.tabPage2.Controls.Add(this.txtTargetFloor);
            this.tabPage2.Controls.Add(this.label6);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage2.Size = new System.Drawing.Size(1644, 670);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "floor";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // btnStopFloor
            // 
            this.btnStopFloor.Location = new System.Drawing.Point(412, 12);
            this.btnStopFloor.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnStopFloor.Name = "btnStopFloor";
            this.btnStopFloor.Size = new System.Drawing.Size(100, 29);
            this.btnStopFloor.TabIndex = 6;
            this.btnStopFloor.Text = "停止";
            this.btnStopFloor.UseVisualStyleBackColor = true;
            this.btnStopFloor.Click += new System.EventHandler(this.btnStopFloor_Click);
            // 
            // labelReviewResult
            // 
            this.labelReviewResult.AutoSize = true;
            this.labelReviewResult.Location = new System.Drawing.Point(545, 19);
            this.labelReviewResult.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelReviewResult.Name = "labelReviewResult";
            this.labelReviewResult.Size = new System.Drawing.Size(67, 15);
            this.labelReviewResult.TabIndex = 5;
            this.labelReviewResult.Text = "评论结果";
            // 
            // txtFloors
            // 
            this.txtFloors.Location = new System.Drawing.Point(97, 80);
            this.txtFloors.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtFloors.Name = "txtFloors";
            this.txtFloors.Size = new System.Drawing.Size(1535, 582);
            this.txtFloors.TabIndex = 4;
            this.txtFloors.Text = "";
            // 
            // btnStartFloor
            // 
            this.btnStartFloor.Location = new System.Drawing.Point(285, 12);
            this.btnStartFloor.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnStartFloor.Name = "btnStartFloor";
            this.btnStartFloor.Size = new System.Drawing.Size(100, 29);
            this.btnStartFloor.TabIndex = 3;
            this.btnStartFloor.Text = "开始";
            this.btnStartFloor.UseVisualStyleBackColor = true;
            this.btnStartFloor.Click += new System.EventHandler(this.btnStartFloor_Click);
            // 
            // lblCurrent
            // 
            this.lblCurrent.AutoSize = true;
            this.lblCurrent.Location = new System.Drawing.Point(43, 80);
            this.lblCurrent.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCurrent.Name = "lblCurrent";
            this.lblCurrent.Size = new System.Drawing.Size(45, 15);
            this.lblCurrent.TabIndex = 2;
            this.lblCurrent.Text = "当前:";
            // 
            // txtTargetFloor
            // 
            this.txtTargetFloor.Location = new System.Drawing.Point(97, 15);
            this.txtTargetFloor.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtTargetFloor.Name = "txtTargetFloor";
            this.txtTargetFloor.Size = new System.Drawing.Size(132, 25);
            this.txtTargetFloor.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(11, 19);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(75, 15);
            this.label6.TabIndex = 0;
            this.label6.Text = "目标楼层:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1652, 699);
            this.Controls.Add(this.tabControl1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "Form1";
            this.Text = "Form1";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericRequestInterval)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericInterval)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericTaskCount)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtJsessid;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox txtParams;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.RichTextBox txtSignRsult;
        private System.Windows.Forms.RichTextBox txtResponse;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.NumericUpDown numericInterval;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericTaskCount;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericRequestInterval;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button btnStartFloor;
        private System.Windows.Forms.Label lblCurrent;
        private System.Windows.Forms.TextBox txtTargetFloor;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.RichTextBox txtFloors;
        private System.Windows.Forms.Label labelReviewResult;
        private System.Windows.Forms.Button btnStopFloor;
    }
}

