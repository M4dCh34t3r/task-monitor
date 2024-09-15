using TaskMonitor.Enums;

namespace TaskMonitor.DTOs
{
    public struct ServiceExceptionDTO
    {
        public ModalTheme Theme { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
    }
}
