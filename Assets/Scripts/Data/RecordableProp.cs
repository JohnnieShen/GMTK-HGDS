public interface RecordableProp
{
    PropStatusFrame CaptureFrame();

    void ApplyFrame(PropStatusFrame frame);
}
