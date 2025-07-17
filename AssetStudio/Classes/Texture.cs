using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public abstract class Texture : NamedObject
    {
        protected Texture(ObjectReader reader) : base(reader)
        {
            if (version[0] >= 2023) // 2023.1 and up (6000 avaliable 2023 not tested)
            {
                var m_IsAlphaChannelOptional = reader.ReadBoolean();
                reader.AlignStream();
            }
            else if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 3)) //2017.3 to 2022.x
            {
                var m_ForcedFallbackFormat = reader.ReadInt32();
                var m_DownscaleFallback = reader.ReadBoolean();
                if (version[0] > 2020 || (version[0] == 2020 && version[1] >= 2)) //2020.2 and up
                {
                    var m_IsAlphaChannelOptional = reader.ReadBoolean();
                }
                reader.AlignStream();
            }
        }
    }
}
