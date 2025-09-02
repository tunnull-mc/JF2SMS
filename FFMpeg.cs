using System.Diagnostics;

public class FFMpeg
{
    public static void Transcode(string FFMpegExecutable, string Input, string Destination, int ThreadCount = 8, int QualityScale = 5, string AspectRatio = "4:3", string AdditionalArguments = "")
    {
        Process.Start(FFMpegExecutable, $"-hwaccel cuda -i \"{Input}\" -v warning -stats -vf scale=640:480 -aspect {AspectRatio} -shortest -vtag xvid -qscale:v {QualityScale} -threads {ThreadCount} \"{Destination}.avi\"").WaitForExit();

    }
}