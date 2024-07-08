using litsdk;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace litqrcode
{
    [litsdk.Action(Name = "二维码识别生成", Category = "图像", IsLinux = false, RefFile = "zxing.dll,zxing.presentation.dll", Order = 120, Description = "生成二维码图片和识别二维码内容")]
    public class QrCodeActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "二维码识别生成";

        [Argument(Name = "操作方式", ControlType = ControlType.ComboBox, Order = 1, Description = "选择是生成还是识别二维码")]
        public QrCodeType QrCodeType { get; set; }

        /// <summary>
        /// 生成内容
        /// </summary>
        [Argument(Name = "生成内容", ControlType = ControlType.TextArea, Order = 2, Description = "生成的文字内容")]
        public string Content { get; set; }

        /// <summary>
        /// 图片大小
        /// </summary>
        [Argument(Name = "图片大小", ControlType = ControlType.NumericUpDown, Order = 3, Description = "生成二维码大小，它是一个正方形")]
        public int ImgSize { get; set; } = 258;

        /// <summary>
        /// 容错率真7%
        /// </summary>
        [Argument(Name = "容错率", ControlType = ControlType.ComboBox, Order = 4, Description = "容错率越大，图片越复杂和体积大")]
        public string ErrorCorrectionLevel { get; set; } = "低 7%";

        /// <summary>
        /// 生成保存路径
        /// </summary>
        [Argument(Name = "保存路径", ControlType = ControlType.File, Order = 6, Description = "生成的二维码保存地址")]
        public string EncodeFilePath { get; set; }


        [Argument(Name = "二维码文件", ControlType = ControlType.File, Order = 6, Description = "需要识别的二维码文件")]
        public string DecodeFilePath { get; set; }

        [Argument(Name = "识别结果", ControlType = ControlType.Variable, Order = 9, Description = "二维码识别结果存于字符变量")]
        public string DecodeVarName { get; set; }


        /// <summary>
        /// https://www.cnblogs.com/huanyun/p/11698424.html
        /// https://www.cnblogs.com/mohanchen/p/7086902.html
        /// </summary>
        /// <param name="context"></param>
        public override void Execute(ActivityContext context)
        {
            Dictionary<EncodeHintType, object> hints = new Dictionary<EncodeHintType, object>();
            hints.Add(EncodeHintType.CHARACTER_SET, "UTF-8");
            hints.Add(EncodeHintType.MARGIN, 1);

            switch (ErrorCorrectionLevel)
            {
                case "低 7%":
                    hints.Add(EncodeHintType.ERROR_CORRECTION, ZXing.QrCode.Internal.ErrorCorrectionLevel.L);
                    break;
                case "中 15%":
                    hints.Add(EncodeHintType.ERROR_CORRECTION, ZXing.QrCode.Internal.ErrorCorrectionLevel.M);
                    break;
                case "中高 25%":
                    hints.Add(EncodeHintType.ERROR_CORRECTION, ZXing.QrCode.Internal.ErrorCorrectionLevel.Q);
                    break;
                case "高 30%":
                    hints.Add(EncodeHintType.ERROR_CORRECTION, ZXing.QrCode.Internal.ErrorCorrectionLevel.H);
                    break;
            }
            if (this.QrCodeType == QrCodeType.Encode)
            {
                string content = context.ReplaceVar(this.Content);
                QRCodeWriter writer = new QRCodeWriter();
                BitMatrix matrix = writer.encode(content, BarcodeFormat.QR_CODE, this.ImgSize, this.ImgSize, hints);
                Bitmap m = toBitmap(matrix);
                string save = context.ReplaceVar(this.EncodeFilePath);
                m.Save(save, System.Drawing.Imaging.ImageFormat.Jpeg);
                context.WriteLog($"生成二维码成功，图片边长{m.Width}");
            }
            else
            {
                string imgfile = context.ReplaceVar(this.DecodeFilePath);
                if (!System.IO.File.Exists(imgfile)) throw new Exception("二维码图片不存在");
                Image image = System.Drawing.Image.FromFile(imgfile);
                Bitmap barcodeBitmap = new Bitmap(image);
                BarcodeReader reader = new BarcodeReader();

                reader.Options.CharacterSet = "UTF-8";

                var result = reader.Decode(barcodeBitmap);

                context.SetVarStr(this.DecodeVarName, "");
                if (result != null)
                {
                    context.SetVarStr(this.DecodeVarName, result.Text);
                    string txt = result.Text.Length > 10 ? $"长度为{result.Text.Length}" : result.Text;
                    context.WriteLog($"二维码识别结果{txt}");
                }
                else
                {
                    context.WriteLog($"二维码识别出错");
                }
            }
        }

        public static Bitmap toBitmap(BitMatrix matrix)
        {
            int width = matrix.Width;
            int height = matrix.Height;
            Bitmap bmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bmap.SetPixel(x, y, matrix[x, y] ? ColorTranslator.FromHtml("0xFF000000") : ColorTranslator.FromHtml("0xFFFFFFFF"));
                }
            }
            return bmap;
        }

        public override void Validate(ActivityContext context)
        {
            if (this.QrCodeType == QrCodeType.Encode)
            {
                if (string.IsNullOrEmpty(this.Content)) throw new Exception("生成二维码内容不得为空");
                if (string.IsNullOrEmpty(this.EncodeFilePath)) throw new Exception("保存文件路径不得为空");
            }
            else
            {
                if (string.IsNullOrEmpty(this.DecodeFilePath)) throw new Exception("识别二维码图片地址不得为空");
                if (string.IsNullOrEmpty(this.DecodeVarName)) throw new Exception("识别结果变量不得为空");
                if (!context.ContainsStr(this.DecodeVarName)) throw new Exception($"识别结果变量名{this.DecodeVarName}不存在");
            }
        }

        public override ControlStyle GetControlStyle(string field)
        {
            ControlStyle style = new ControlStyle() { Variables = ControlStyle.GetVariables(true, false, true) };
            switch (field)
            {
                case "DecodeVarName":
                    style.Variables = ControlStyle.GetVariables(true);
                    style.Visible = this.QrCodeType == QrCodeType.Decode;
                    break;
                case "Content":
                case "EncodeFilePath":
                case "ImgSize":
                    style.Visible = this.QrCodeType == QrCodeType.Encode;
                    style.Max = 1000;
                    style.Min = 10;
                    break;
                case "ErrorCorrectionLevel":
                    style.Visible = this.QrCodeType == QrCodeType.Encode;
                    style.DropDownList = new List<string>() { "低 7%", "中 15%", "中高 25%", "高 30%" };
                    break;
                case "DecodeFilePath":
                    style.Visible = this.QrCodeType == QrCodeType.Decode;
                    break;
            }
            return style;
        }
    }
}