/**
 * DiagnosisChartsManager - Manages radar charts for diagnosis visualization
 */
class DiagnosisChartsManager {
    constructor() {
        this.charts = new Map();
        this.initializeCharts();
    }

    /**
     * Initialize all charts on the page
     */
    initializeCharts() {
        const chartElements = document.querySelectorAll('[data-diagnosis-chart]');
        
        if (chartElements.length === 0) {
            console.warn('No diagnosis charts found on the page');
            return;
        }

        chartElements.forEach(element => {
            try {
                const chartId = element.dataset.diagnosisChart;
                const chartDataJson = element.dataset.chartData;
                
                if (!chartDataJson) {
                    console.error(`No chart data found for chart ${chartId}`);
                    return;
                }

                const chartData = JSON.parse(chartDataJson);
                this.renderRadarChart(element, chartData);
            } catch (error) {
                console.error('Error initializing chart:', error);
            }
        });
    }

    /**
     * Render a radar chart in the specified container
     * @param {HTMLElement} container - The container element
     * @param {Object} data - The chart data
     */
    renderRadarChart(container, data) {
        // Initialize ECharts instance
        const chart = echarts.init(container);
        
        // Build indicator array for radar chart
        const indicators = data.labels.map((label, index) => ({
            name: label,
            max: data.maxScore || 10,
            axisLabel: {
                show: true,
                fontSize: 12
            }
        }));

        // Configure chart options
        const option = {
            // Title removed - displayed in HTML container instead
            tooltip: {
                trigger: 'item',
                formatter: function(params) {
                    if (params.componentType !== 'series') {
                        return '';
                    }
                    
                    const dataIndex = params.dataIndex || 0;
                    const questions = data.questions || [];
                    
                    let tooltipHtml = '<div style="padding: 10px;">';
                    tooltipHtml += `<strong>${data.blockName}</strong><br/>`;
                    
                    if (questions.length > 0) {
                        data.labels.forEach((label, index) => {
                            const question = questions[index];
                            if (question) {
                                tooltipHtml += `<div style="margin-top: 8px;">`;
                                tooltipHtml += `<strong>${label}:</strong> ${params.value[index] || 0}<br/>`;
                                tooltipHtml += `<span style="color: #666; font-size: 12px;">${question.text}</span><br/>`;
                                tooltipHtml += `<span style="color: #999; font-size: 11px;">Fuente: ${question.source}</span>`;
                                tooltipHtml += `</div>`;
                            }
                        });
                    }
                    
                    tooltipHtml += '</div>';
                    return tooltipHtml;
                }
            },
            radar: {
                indicator: indicators,
                shape: 'circle',
                splitNumber: 5,
                radius: '75%',
                center: ['50%', '50%'],
                axisName: {
                    color: '#666',
                    fontSize: 12,
                    formatter: function(value) {
                        // Truncate long labels
                        return value.length > 10 ? value.substring(0, 10) + '...' : value;
                    }
                },
                splitLine: {
                    lineStyle: {
                        color: [
                            'rgba(238, 197, 102, 0.1)',
                            'rgba(238, 197, 102, 0.2)',
                            'rgba(238, 197, 102, 0.3)',
                            'rgba(238, 197, 102, 0.4)',
                            'rgba(238, 197, 102, 0.5)'
                        ].reverse()
                    }
                },
                splitArea: {
                    show: true,
                    areaStyle: {
                        color: ['rgba(34, 126, 230, 0.05)', 'rgba(34, 126, 230, 0.1)'],
                        shadowBlur: 10,
                        shadowColor: 'rgba(0, 0, 0, 0.1)'
                    }
                },
                axisLine: {
                    lineStyle: {
                        color: 'rgba(238, 197, 102, 0.5)'
                    }
                }
            },
            series: [{
                name: data.blockName,
                type: 'radar',
                data: [{
                    value: data.scores,
                    name: data.blockName,
                    symbol: 'circle',
                    symbolSize: 5,
                    lineStyle: {
                        width: 2,
                        color: '#227ee6'
                    },
                    areaStyle: {
                        opacity: 0.3,
                        color: 'rgba(34, 126, 230, 0.5)'
                    },
                    itemStyle: {
                        color: '#227ee6'
                    }
                }]
            }],
            grid: {
                left: '3%',
                right: '4%',
                bottom: '3%',
                containLabel: true
            }
        };

        // Set option and handle resize
        chart.setOption(option);
        
        // Store chart reference
        const chartId = container.dataset.diagnosisChart;
        this.charts.set(chartId, chart);

        // Handle window resize
        window.addEventListener('resize', () => {
            chart.resize();
        });
    }

    /**
     * Trigger print view
     */
    printView() {
        window.print();
    }

    /**
     * Resize all charts
     */
    resizeCharts() {
        this.charts.forEach(chart => {
            chart.resize();
        });
    }

    /**
     * Destroy all charts and clean up
     */
    destroy() {
        this.charts.forEach(chart => {
            chart.dispose();
        });
        this.charts.clear();
    }
}

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    if (typeof echarts !== 'undefined') {
        window.diagnosisChartsManager = new DiagnosisChartsManager();
    } else {
        console.error('ECharts library not loaded. Please include echarts.min.js before this script.');
    }
});

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = DiagnosisChartsManager;
}