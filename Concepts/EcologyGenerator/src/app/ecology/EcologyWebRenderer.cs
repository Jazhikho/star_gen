using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using StarGen.Domain.Ecology;

namespace StarGen.App.Ecology
{
    /// <summary>
    /// Renders the ecology web as a visual graph with nodes and connections.
    /// </summary>
    public partial class EcologyWebRenderer : Control
    {
        private EcologyWeb? _web;
        private Dictionary<Guid, Vector2> _nodePositions = new Dictionary<Guid, Vector2>();
        private Dictionary<Guid, Color> _nodeColors = new Dictionary<Guid, Color>();
        private TrophicSlot? _hoveredSlot;

        private const float NODE_RADIUS = 15f;
        private const float LEVEL_SPACING = 100f;
        private const float NODE_SPACING = 80f;
        private const float CONNECTION_WIDTH = 1.5f;

        private static readonly Dictionary<TrophicLevel, Color> LevelColors = new Dictionary<TrophicLevel, Color>
        {
            { TrophicLevel.Producer, new Color(0.2f, 0.8f, 0.2f) },
            { TrophicLevel.PrimaryConsumer, new Color(0.4f, 0.6f, 0.9f) },
            { TrophicLevel.SecondaryConsumer, new Color(0.9f, 0.7f, 0.2f) },
            { TrophicLevel.TertiaryConsumer, new Color(0.9f, 0.4f, 0.2f) },
            { TrophicLevel.ApexPredator, new Color(0.9f, 0.2f, 0.2f) },
            { TrophicLevel.Decomposer, new Color(0.6f, 0.4f, 0.2f) }
        };

        public override void _Ready()
        {
            MouseFilter = MouseFilterEnum.Stop;
        }

        public override void _GuiInput(InputEvent @event)
        {
            InputEventMouseMotion? mouseMotion = @event as InputEventMouseMotion;
            if (mouseMotion != null)
            {
                UpdateHoveredSlot(mouseMotion.Position);
            }
        }

        /// <summary>
        /// Renders the given ecology web.
        /// </summary>
        public void RenderWeb(EcologyWeb web)
        {
            _web = web;
            CalculateNodePositions();
            QueueRedraw();
        }

        public override void _Draw()
        {
            if (_web == null)
            {
                return;
            }

            DrawConnections();
            DrawNodes();
            DrawLegend();
            DrawHoverInfo();
        }

        private void CalculateNodePositions()
        {
            _nodePositions.Clear();
            _nodeColors.Clear();

            if (_web == null)
            {
                return;
            }

            float centerX = Size.X / 2f;
            float startY = Size.Y - 50f;

            Dictionary<TrophicLevel, List<TrophicSlot>> slotsByLevel = new Dictionary<TrophicLevel, List<TrophicSlot>>();
            foreach (TrophicLevel level in Enum.GetValues(typeof(TrophicLevel)))
            {
                slotsByLevel[level] = _web.GetSlotsByLevel(level);
            }

            TrophicLevel[] levelOrder = new TrophicLevel[]
            {
                TrophicLevel.Producer,
                TrophicLevel.Decomposer,
                TrophicLevel.PrimaryConsumer,
                TrophicLevel.SecondaryConsumer,
                TrophicLevel.TertiaryConsumer,
                TrophicLevel.ApexPredator
            };

            int visualLevel = 0;
            foreach (TrophicLevel level in levelOrder)
            {
                List<TrophicSlot> slots = slotsByLevel[level];
                if (slots.Count == 0)
                {
                    continue;
                }

                float y = startY - visualLevel * LEVEL_SPACING;

                if (level == TrophicLevel.Decomposer)
                {
                    y = startY;
                    float decompStartX = centerX + 200f;
                    for (int i = 0; i < slots.Count; i++)
                    {
                        Vector2 pos = new Vector2(decompStartX + i * NODE_SPACING, y);
                        _nodePositions[slots[i].Id] = pos;
                        _nodeColors[slots[i].Id] = LevelColors[level];
                    }
                }
                else
                {
                    float totalWidth = (slots.Count - 1) * NODE_SPACING;
                    float startX = centerX - totalWidth / 2f;

                    for (int i = 0; i < slots.Count; i++)
                    {
                        Vector2 pos = new Vector2(startX + i * NODE_SPACING, y);
                        _nodePositions[slots[i].Id] = pos;
                        _nodeColors[slots[i].Id] = LevelColors[level];
                    }

                    visualLevel += 1;
                }
            }
        }

        private void DrawConnections()
        {
            if (_web == null) return;
            foreach (TrophicConnection conn in _web.Connections)
            {
                if (!_nodePositions.ContainsKey(conn.SourceSlotId) || !_nodePositions.ContainsKey(conn.TargetSlotId))
                {
                    continue;
                }

                Vector2 from = _nodePositions[conn.SourceSlotId];
                Vector2 to = _nodePositions[conn.TargetSlotId];

                float alpha = 0.2f + conn.Strength * 0.5f;
                Color color = new Color(0.5f, 0.5f, 0.5f, alpha);

                if (_hoveredSlot != null && (conn.SourceSlotId == _hoveredSlot.Id || conn.TargetSlotId == _hoveredSlot.Id))
                {
                    color = new Color(1f, 1f, 0f, 0.8f);
                }

                DrawLine(from, to, color, CONNECTION_WIDTH);

                Vector2 dir = (to - from).Normalized();
                Vector2 arrowPos = to - dir * NODE_RADIUS;
                Vector2 perpendicular = new Vector2(-dir.Y, dir.X);
                float arrowSize = 8f;

                DrawLine(arrowPos, arrowPos - dir * arrowSize + perpendicular * arrowSize * 0.5f, color, CONNECTION_WIDTH);
                DrawLine(arrowPos, arrowPos - dir * arrowSize - perpendicular * arrowSize * 0.5f, color, CONNECTION_WIDTH);
            }
        }

        private void DrawNodes()
        {
            if (_web == null) return;
            foreach (TrophicSlot slot in _web.Slots)
            {
                if (!_nodePositions.ContainsKey(slot.Id))
                {
                    continue;
                }

                Vector2 pos = _nodePositions[slot.Id];
                Color color = _nodeColors[slot.Id];

                float radius = NODE_RADIUS + (slot.BiomassCapacity / 100f) * 5f;
                radius = Mathf.Clamp(radius, NODE_RADIUS, NODE_RADIUS * 2f);

                if (_hoveredSlot != null && _hoveredSlot.Id == slot.Id)
                {
                    DrawCircle(pos, radius + 4f, Colors.Yellow);
                }

                DrawCircle(pos, radius, color);

                DrawArc(pos, radius, 0f, Mathf.Tau, 32, Colors.White, 1.5f);
            }
        }

        private void DrawLegend()
        {
            float x = 10f;
            float y = 10f;
            float spacing = 25f;

            foreach (KeyValuePair<TrophicLevel, Color> kvp in LevelColors)
            {
                DrawCircle(new Vector2(x + 8f, y + 8f), 8f, kvp.Value);
                DrawString(ThemeDB.FallbackFont, new Vector2(x + 25f, y + 12f), kvp.Key.ToString(), HorizontalAlignment.Left, -1f, 12, Colors.White);
                y += spacing;
            }
        }

        private void DrawHoverInfo()
        {
            if (_hoveredSlot == null)
            {
                return;
            }

            Vector2 pos = _nodePositions[_hoveredSlot.Id];
            Vector2 infoPos = pos + new Vector2(20f, -60f);

            Vector2 textSize = new Vector2(180f, 80f);
            DrawRect(new Rect2(infoPos - new Vector2(5f, 5f), textSize + new Vector2(10f, 10f)), new Color(0f, 0f, 0f, 0.8f));

            float lineHeight = 16f;
            DrawString(ThemeDB.FallbackFont, infoPos, _hoveredSlot.Description, HorizontalAlignment.Left, -1f, 14, Colors.White);
            DrawString(ThemeDB.FallbackFont, infoPos + new Vector2(0f, lineHeight), "Level: " + _hoveredSlot.Level, HorizontalAlignment.Left, -1f, 12, Colors.LightGray);
            DrawString(ThemeDB.FallbackFont, infoPos + new Vector2(0f, lineHeight * 2f), "Niche: " + _hoveredSlot.Niche, HorizontalAlignment.Left, -1f, 12, Colors.LightGray);
            DrawString(ThemeDB.FallbackFont, infoPos + new Vector2(0f, lineHeight * 3f), "Biomass: " + _hoveredSlot.BiomassCapacity.ToString("F1"), HorizontalAlignment.Left, -1f, 12, Colors.LightGray);
            DrawString(ThemeDB.FallbackFont, infoPos + new Vector2(0f, lineHeight * 4f), "Prey: " + _hoveredSlot.PreySlotIds.Count + ", Predators: " + _hoveredSlot.PredatorSlotIds.Count, HorizontalAlignment.Left, -1f, 12, Colors.LightGray);
        }

        private void UpdateHoveredSlot(Vector2 mousePos)
        {
            TrophicSlot? newHovered = null;

            if (_web != null)
            {
                foreach (TrophicSlot slot in _web.Slots)
                {
                    if (!_nodePositions.ContainsKey(slot.Id))
                    {
                        continue;
                    }

                    Vector2 pos = _nodePositions[slot.Id];
                    if (pos.DistanceTo(mousePos) <= NODE_RADIUS + 5f)
                    {
                        newHovered = slot;
                        break;
                    }
                }
            }

            if (newHovered != _hoveredSlot)
            {
                _hoveredSlot = newHovered;
                QueueRedraw();
            }
        }
    }
}

