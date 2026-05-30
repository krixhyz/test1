import { jsPDF } from 'jspdf';
import html2canvas from 'html2canvas';

function hexToRgb(hex) {
  const value = String(hex || '').replace('#', '').trim();
  if (!/^[0-9a-fA-F]{6}$/.test(value)) return [22, 163, 74];
  return [
    parseInt(value.slice(0, 2), 16),
    parseInt(value.slice(2, 4), 16),
    parseInt(value.slice(4, 6), 16),
  ];
}

export async function exportElementToPdf({
  element,
  fileName,
  title,
  subtitle,
  accent = '#16a34a',
  scale = 2,
}) {
  if (!element) return;

  const canvas = await html2canvas(element, {
    scale,
    backgroundColor: '#f9fafb',
    useCORS: true,
    scrollY: -window.scrollY,
    windowWidth: element.scrollWidth,
    windowHeight: element.scrollHeight,
  });

  const pdf = new jsPDF('p', 'mm', 'a4');
  const pageWidth = pdf.internal.pageSize.getWidth();
  const pageHeight = pdf.internal.pageSize.getHeight();
  const marginX = 12;
  const marginTop = 12;
  const headerHeight = 24;
  const footerHeight = 10;
  const contentWidth = pageWidth - marginX * 2;
  const contentTop = marginTop + headerHeight + 6;
  const contentBottom = pageHeight - footerHeight - 10;
  const contentHeight = contentBottom - contentTop;

  const imageWidth = contentWidth;
  const imageHeight = (canvas.height * imageWidth) / canvas.width;
  const imageData = canvas.toDataURL('image/png', 1.0);
  const pageCount = Math.max(1, Math.ceil(imageHeight / contentHeight));
  const [accentR, accentG, accentB] = hexToRgb(accent);

  const drawHeader = (pageNumber) => {
    pdf.setFillColor(accentR, accentG, accentB);
    pdf.roundedRect(marginX, marginTop, contentWidth, headerHeight, 3, 3, 'F');
    pdf.setTextColor(255, 255, 255);
    pdf.setFont('helvetica', 'bold');
    pdf.setFontSize(16);
    pdf.text(title, marginX + 8, marginTop + 10);
    if (subtitle) {
      pdf.setFont('helvetica', 'normal');
      pdf.setFontSize(9.5);
      pdf.text(subtitle, marginX + 8, marginTop + 18);
    }
    pdf.setFillColor(255, 255, 255);
    pdf.roundedRect(pageWidth - marginX - 32, marginTop + 6, 22, 10, 2, 2, 'F');
    pdf.setTextColor(accentR, accentG, accentB);
    pdf.setFont('helvetica', 'bold');
    pdf.setFontSize(8.5);
    pdf.text(`Page ${pageNumber}`, pageWidth - marginX - 21, marginTop + 12.8, { align: 'center' });
  };

  for (let pageIndex = 0; pageIndex < pageCount; pageIndex += 1) {
    if (pageIndex > 0) pdf.addPage();

    drawHeader(pageIndex + 1);

    const offsetY = contentTop - (pageIndex * contentHeight);
    pdf.addImage(imageData, 'PNG', marginX, offsetY, imageWidth, imageHeight);

    pdf.setDrawColor(226, 232, 240);
    pdf.line(marginX, pageHeight - 9, pageWidth - marginX, pageHeight - 9);
    pdf.setTextColor(100, 116, 139);
    pdf.setFont('helvetica', 'normal');
    pdf.setFontSize(8);
    pdf.text(`Generated ${new Date().toLocaleString('en-GB')}`, marginX, pageHeight - 4);
  }

  pdf.save(fileName);
}