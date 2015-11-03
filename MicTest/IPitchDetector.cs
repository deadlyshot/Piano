using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicTest {
    public interface IPitchDetector {
        float DetectPitch (float[] buffer, int frames);
    }
}
