﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Fireflies {
    interface IOrchestrator {
        void Update(TimeSpan frameTime, Color[] leds);
    }
}