using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace SendMseeageByLineNotify
{
    class ImageTools
    {
        //使用方法調用GenerateHighThumbnail()方法即可
        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldImagePath">表示要被縮放的圖片路徑</param>
        /// <param name="newImagePath">表示縮放後保存的圖片路徑</param>
        /// <param name="width">縮放範圍寬</param>
        /// <param name="height">縮放範圍高</param>
        public void GenerateHighThumbnail(string oldImagePath, string newImagePath, int width, int height)
        {
            Image oldImage = Image.FromFile(oldImagePath);
            int newWidth = AdjustSize(width, height, oldImage.Width, oldImage.Height).Width;
            int newHeight = AdjustSize(width, height, oldImage.Width, oldImage.Height).Height;
            Image thumbnailImage = oldImage.GetThumbnailImage(newWidth, newHeight, new Image.GetThumbnailImageAbort(ThumbnailCallback), IntPtr.Zero);
            Bitmap bm = new Bitmap(thumbnailImage);
            //處理JPG質量的函數
            ImageCodecInfo ici = GetEncoderInfo("image/jpeg");
            if (ici != null)
            {
                EncoderParameters ep = new EncoderParameters(1);
                ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)100);
                bm.Save(newImagePath, ici, ep);
                //釋放所有資源，不釋放，可能會出錯誤。
                ep.Dispose();
                ep = null;
            }
            ici = null;
            bm.Dispose();
            bm = null;
            thumbnailImage.Dispose();
            thumbnailImage = null;
            oldImage.Dispose();
            oldImage = null;
        }
        private bool ThumbnailCallback()
        {
            return false;
        }
        private ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }
        public struct PicSize
        {
            public int Width;
            public int Height;
        }
        public PicSize AdjustSize(int spcWidth, int spcHeight, int orgWidth, int orgHeight)
        {
            PicSize size = new PicSize();
            // 原始寬高在指定寬高範圍內，不作任何處理 
            if (orgWidth <= spcWidth && orgHeight <= spcHeight)
            {
                size.Width = orgWidth;
                size.Height = orgHeight;
            }
            else
            {
                // 取得比例系數 
                float w = orgWidth / (float)spcWidth;
                float h = orgHeight / (float)spcHeight;
                // 寬度比大於高度比 
                if (w > h)
                {
                    size.Width = spcWidth;
                    size.Height = (int)(w >= 1 ? Math.Round(orgHeight / w) : Math.Round(orgHeight * w));
                }
                // 寬度比小於高度比 
                else if (w < h)
                {
                    size.Height = spcHeight;
                    size.Width = (int)(h >= 1 ? Math.Round(orgWidth / h) : Math.Round(orgWidth * h));
                }
                // 寬度比等於高度比 
                else
                {
                    size.Width = spcWidth;
                    size.Height = spcHeight;
                }
            }
            return size;
        }
    }
}
