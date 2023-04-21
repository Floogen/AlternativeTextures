using System;

namespace AlternativeTextures.Framework.Interfaces
{
    public interface IJsonAssetsApi
    {
        int GetBigCraftableId(string name);
        int GetObjectId(string name);

        event EventHandler IdsAssigned;
    }
}
