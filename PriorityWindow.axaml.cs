using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Voroncov1.Models;

namespace Voroncov1
{
    public partial class PriorityWindow : Window
    {
        private int AgentId;
        public int? Priority { get; private set; }

        public PriorityWindow(int agentId)
        {
            InitializeComponent();
            AgentId = agentId;
        }

        private void OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (int.TryParse(PriorityTextBox.Text, out int priority))
            {
                Priority = priority;
                using var ctx = new DatabaseContext();
                ctx.Agents.Where(a => a.Id == AgentId)
                    .ExecuteUpdate(setters => setters.SetProperty(a => a.Priority, Priority));
                ctx.SaveChanges();
                Close();
            }
            else
            {
                Console.WriteLine("Invalid priority number");
            }
        }
    }
}