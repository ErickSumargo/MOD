using System;

namespace Opticus
{
    class ILabel
    {
        /*----------------------------------------Declaring Local Variables-----------------------------------------*/

        public int Name { get; set; }

        public int Rank { get; set; }

        /*----------------------------------------------------------------------------------------------------------*/

        /*-------------------------------------------Declaring SubClasses-------------------------------------------*/

        public ILabel Root { get; set; }

        /*----------------------------------------------------------------------------------------------------------*/

        public ILabel(int Name)
        {
            this.Name = Name;
            Rank = 0;

            Root = this;
        }

        public ILabel GetRoot()
        {
            var thisObj = this;
            var root = Root;

            while (thisObj != root)
            {
                thisObj = root;
                root = root.Root;
            }

            Root = root;

            return Root;
        }

        public void Join(ILabel root2)
        {
            if (root2.Rank < Rank)
            {
                root2.Root = this;
            }

            else
            {
                Root = root2;

                if (Rank == root2.Rank)
                {
                    root2.Rank++;
                }
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as ILabel;

            if (other == null)
            {
                return false;
            }

            return other.Name == Name;
        }

        public override int GetHashCode()
        {
            return Name;
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }
}