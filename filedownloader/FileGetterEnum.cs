using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace filedownloader
{
    public enum FileGetterEnum
    {
        CREATING_REQUEST,
        APPENDING_PROXY,
        APPENDING_RANGE,
        STARTING_REQUEST,
        REQUEST_COMPUTING_SIZE,
        REQUEST_STARTING_DOWNLOAD,
        REQUEST_DOWNLOADING,
        REQUEST_DOWNLOAD_FINISHED
    }
}
