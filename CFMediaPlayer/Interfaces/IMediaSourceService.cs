﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFMediaPlayer.Interfaces
{
    public interface IMediaSourceService
    {
        List<IMediaSource> GetAll();
    }
}
