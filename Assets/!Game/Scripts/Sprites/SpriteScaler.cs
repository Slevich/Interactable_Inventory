using UnityEngine;

public static class SpriteScaler
{
    public static void ScaleSpriteFromRenderer(SpriteRenderer Renderer, Vector2 NewSize)
    {
        if(Renderer == null)
            return;
        
        switch (Renderer.drawMode)
        {
            case SpriteDrawMode.Simple:
            {
                if (Renderer.sprite == null)
                    return;

                Vector2 spriteSize = Renderer.sprite.bounds.size;
                if (spriteSize.x <= 0 || spriteSize.y <= 0)
                    return;

                Vector3 newScale = new Vector3(
                    NewSize.x / spriteSize.x,
                    NewSize.y / spriteSize.y,
                    1f
                );

                Renderer.transform.localScale = newScale;
                break;
            }

            case SpriteDrawMode.Sliced:
            case SpriteDrawMode.Tiled:
            {
                Renderer.size = NewSize;
                Renderer.transform.localScale = Vector3.one;
                break;
            }

            default:
            {
                Debug.LogWarning($"Unsupported SpriteDrawMode: {Renderer.drawMode}");
                break;
            }
        }
    }
}
