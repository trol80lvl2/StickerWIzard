using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StickerWizard
{
    public class PdfPageDisposable : IDisposable
    {
        public PdfPage Page { get; private set; }

        public PdfPageDisposable(PdfPage page) => Page = (Page == null) ? page : Page;

        public void Dispose() => Page?.Close();
    }
}
