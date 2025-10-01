using UnityEngine;

public interface IHighlightable
{
    void Highlight(Color color, int highlightLayer);
    void ClearHighlight();
    bool IsHighlighted { get; }
}
