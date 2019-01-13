using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterCourses
{
    class Person
    {
        public string faceId;
        public FaceRectangle facerectangle;
        public FaceAttributes faceAttributes;

        public Boolean getAttention()
        {
            if (faceAttributes.emotion.happiness > faceAttributes.emotion.sadness)
                return true;
            return false;

        }
    }
}
