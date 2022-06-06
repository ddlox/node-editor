using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SG_Administrator
{
    public class route_design
    {
        private int m_vpos = 0;
        private int m_offset = 12;

        public route_design()
        {
        }

        public void set_route_vpos(int vpos)
        {
            m_vpos = vpos;
        }

        public int get_next_route_vpos()
        {
            m_vpos += m_offset;
            return m_vpos;
        }

        public void remove_route_vpos()
        {
            m_vpos -= m_offset;
        }
    }
}
