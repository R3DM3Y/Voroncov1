using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Voroncov1.Models;
using Path = System.IO.Path;

namespace Voroncov1;

public partial class MainWindow : Window
{
    private static List<Agent> agentsForPhotoAndPriority;
    ObservableCollection<AgentYearlySalesPresenter> agents = new ObservableCollection<AgentYearlySalesPresenter>();
    List<AgentYearlySalesPresenter> dataSourceAgents;
    private int currentPage = 1;
    private int itemsPerPage = 10;

    public MainWindow()
    {
        InitializeComponent();

        using var ctx = new DatabaseContext();
        
        agentsForPhotoAndPriority = ctx.Agents.ToList();
        dataSourceAgents = ctx.Agents
            .GroupJoin(ctx.ProductSales, agent => agent.Id, sale => sale.AgentId, (agent, sales) => new { Agent = agent, Sales = sales })
            .SelectMany(group => group.Sales
                .GroupBy(sale => sale.SaleDate.Year)
                .Select(yearGroup => new AgentYearlySalesPresenter
                {
                    Id = group.Agent.Id,
                    Email = group.Agent.Email,
                    Phone = group.Agent.Phone,
                    Title = group.Agent.Title,
                    Year = yearGroup.Key,
                    countSales = yearGroup.Sum(sale => sale.ProductCount),
                    totalSalesAmount = yearGroup.Sum(sale => sale.ProductCount * sale.Product.MinCostForAgent),
                    sale = SwitchSale(yearGroup.Sum(sale => sale.ProductCount * sale.Product.MinCostForAgent)),
                    PhotoPath = GetPhotoPath(group.Agent.Id),
                    priority = group.Agent.Priority,
                    typeAgent = ctx.AgentTypes.Where(t => t.Id == group.Agent.AgentTypeId).Select(t => t.Title).FirstOrDefault(),
                }))
            .ToList();
        ListBox.ItemsSource = agents;
        DisplayAgents();
    }

    public void DisplayAgents()
    {
        var temp = dataSourceAgents;
        agents.Clear();
        if (!string.IsNullOrEmpty(SearchTextBox.Text))
        {
            var search = SearchTextBox.Text;
            temp = temp.Where(it => IsContains(it.Email, it.Phone, search)).ToList();
        }
        
        switch (TitleAgentSortComboBox.SelectedIndex)
        {
            case 1: temp = temp.OrderByDescending(it => it.Title).ToList(); break;
            case 0: temp = temp; break;
            case 2: temp = temp.OrderBy(it => it.Title).ToList(); break;
        }

        switch (SaleAgentSortComboBox.SelectedIndex)
        {
            case 1: temp = temp.OrderByDescending(it => it.sale).ToList(); break;
            case 0: temp = temp; break;
            case 2: temp = temp.OrderBy(it => it.sale).ToList(); break;
        }
        switch (PriorityAgentSortComboBox.SelectedIndex)
        {
            case 1: temp = temp.OrderByDescending(it => it.priority).ToList(); break;
            case 0: temp = temp; break; 
            case 2: temp = temp.OrderBy(it => it.priority).ToList(); break;
        }
        switch (TypeAgentFilterCombobox.SelectedIndex)
        {
            case 1: temp = temp.Where(it => it.typeAgent == "ООО").ToList(); break;
            case 0: temp = temp; break;
            case 2: temp = temp.Where(it => it.typeAgent == "МФО").ToList(); break;
            case 3: temp = temp.Where(it => it.typeAgent == "ЗАО").ToList(); break;
            case 4: temp = temp.Where(it => it.typeAgent == "МКК").ToList(); break;
            case 5: temp = temp.Where(it => it.typeAgent == "ПАО").ToList(); break;
            case 6: temp = temp.Where(it => it.typeAgent == "ОАО").ToList(); break;
            default: break;  
        }
        
        int totalItems = temp.Count;
        int totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);

        if (currentPage > totalPages)
        {
            currentPage = totalPages;
        }
        if (currentPage < 1)
        {
            currentPage = 1;
        }

        var paginatedList = temp.Skip((currentPage - 1) * itemsPerPage).Take(itemsPerPage).ToList();

        foreach (var item in paginatedList)
        {
            agents.Add(item);
        }
        
    }
    
    private void NextPage(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        currentPage++;
        DisplayAgents();
    }

    private void PreviousPage(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        currentPage--;
        DisplayAgents();
    }
    
    public bool IsContains(string title, string? description, string search)
    {
        string desc = string.Empty;
        if (description != null) desc = description;
        string message = (title + desc).ToLower();
        search = search.ToLower();
        return message.Contains(search);
    }
    
    private static string SwitchSale(decimal amount)
    {
        return amount switch
        {
            < 10000 => "0%",
            >= 10000 and < 50000 => "5%",
            >= 50000 and < 150000 => "10%",
            >= 150000 and < 500000 => "20%",
            > 500000 => "25%"
        };
    }

    private static string GetPhotoPath(int agentId)
    {
        string absolutePath = "";
        var path = agentsForPhotoAndPriority.Where(it => it.Id == agentId).Select(it => it.Logo).FirstOrDefault();
        Console.WriteLine($"id: {agentId}, path: {path}");
        if (path == null)
        {
            path = "agents/m.jpeg";
        }
        absolutePath = Path.Combine(AppContext.BaseDirectory, path);
        return absolutePath; 
    }

    public class AgentYearlySalesPresenter
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Title { get; set; } = null!;
        public int Year { get; set; }
        public int countSales { get; set; }
        public decimal totalSalesAmount { get; set; }
        public string sale { get; set; }
        public string PhotoPath { get; set; }
        public int priority { get; set; }
        public string typeAgent { get; set; }
        Bitmap? Image
        {
            get
            {
                try
                {
                    string absolutePath = Path.Combine(AppContext.BaseDirectory, PhotoPath);
                    return new Bitmap(absolutePath);
                }
                catch
                {
                    return null;
                }
            }
        }
    }

    private void SearchTextBox_OnTextChanging(object? sender, TextChangingEventArgs e)
    {
        DisplayAgents();
    }

    private void TitleAgentSortComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        DisplayAgents();
    }

    private void TypeAgentFilterCombobox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        DisplayAgents();
    }

    private async void AddAgent_OnClick(object? sender, RoutedEventArgs e)
    {
        using var ctx = new DatabaseContext();
        var newAgent = await new AddAgentWindow().ShowDialog<Agent>(this);
    
        if (newAgent != null)
        {
            ctx.Agents.Add(newAgent);
            await ctx.SaveChangesAsync();
            agentsForPhotoAndPriority = ctx.Agents.ToList();

            var presenter = new AgentYearlySalesPresenter
            {
                Id = newAgent.Id,
                Email = newAgent.Email,
                Phone = newAgent.Phone,
                Title = newAgent.Title,
                Year = DateTime.Now.Year,
                countSales = 0,
                totalSalesAmount = 0,
                sale = "0%",
                PhotoPath = GetPhotoPath(newAgent.Id),
                priority = newAgent.Priority,
                typeAgent = ctx.AgentTypes.Where(t => t.Id == newAgent.AgentTypeId).Select(t => t.Title).FirstOrDefault()
            };
                
                Console.WriteLine(newAgent.Logo);
                Console.WriteLine(presenter.PhotoPath);

                dataSourceAgents.Add(presenter);
        }
    }

    private async void EditMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        using var ctx = new DatabaseContext();
        if (ListBox.SelectedItem is AgentYearlySalesPresenter selectedAgentPresenter)
        {
            var agent = ctx.Agents.FirstOrDefault(a => a.Id == selectedAgentPresenter.Id);
            EditAgentWindow editAgentWindow = new EditAgentWindow(agent);
            var updatedAgent = await editAgentWindow.ShowDialog<Agent>(this);
            if (updatedAgent != null)
            {
                ctx.Agents.Update(updatedAgent);
                await ctx.SaveChangesAsync();
                agentsForPhotoAndPriority = ctx.Agents.ToList();
                
                var oldPresenter = dataSourceAgents.FirstOrDefault(a => a.Id == updatedAgent.Id);
                if (oldPresenter != null)
                {
                    dataSourceAgents.Remove(oldPresenter);
                }

                var newPresenter = new AgentYearlySalesPresenter
                {
                    Id = updatedAgent.Id,
                    Email = updatedAgent.Email,
                    Phone = updatedAgent.Phone,
                    Title = updatedAgent.Title,
                    Year = DateTime.Now.Year,
                    countSales = 0,
                    totalSalesAmount = 0,
                    sale = "0%",
                    PhotoPath = GetPhotoPath(updatedAgent.Id),
                    priority = updatedAgent.Priority,
                    typeAgent = ctx.AgentTypes.Where(t => t.Id == updatedAgent.AgentTypeId).Select(t => t.Title).FirstOrDefault()
                };
                
                Console.WriteLine(updatedAgent.Logo);
                Console.WriteLine(newPresenter.PhotoPath);

                dataSourceAgents.Add(newPresenter);
                DisplayAgents();
            }
        }
    }
    
    private async void PriorityMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ListBox.SelectedItem is AgentYearlySalesPresenter selectedAgentPresenter)
        {
            PriorityWindow priorityWindow = new PriorityWindow(selectedAgentPresenter.Id);
            await priorityWindow.ShowDialog(this);

            if (priorityWindow.Priority.HasValue)
            {
                Console.WriteLine($"Новый приоритет: {priorityWindow.Priority.Value}");

                using var context = new DatabaseContext();
                var agent = await context.Agents.FindAsync(selectedAgentPresenter.Id);
                if (agent != null)
                {
                    agent.Priority = priorityWindow.Priority.Value;
                    await context.SaveChangesAsync();
                }

                var agentToUpdate = dataSourceAgents.FirstOrDefault(a => a.Id == selectedAgentPresenter.Id);
                if (agentToUpdate != null)
                {
                    agentToUpdate.priority = priorityWindow.Priority.Value;
                }
                DisplayAgents();
            }
        }
    }
}