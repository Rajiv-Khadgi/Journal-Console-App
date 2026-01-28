// Simple charts.js
function createSimpleChart(containerId, labels, data, colors) {
    console.log("Looking for container: " + containerId);

    // Try multiple times to find container
    function tryCreateChart(attempt) {
        if (attempt > 5) {
            console.error("Container not found after 5 attempts: " + containerId);
            return;
        }

        var container = document.getElementById(containerId);
        if (!container) {
            console.log("Container not found, attempt " + attempt);
            setTimeout(() => tryCreateChart(attempt + 1), 200);
            return;
        }

        console.log("Container found, creating chart");

        // Clear container
        container.innerHTML = '';

        // Create canvas
        var canvas = document.createElement('canvas');
        canvas.style.width = '100%';
        canvas.style.height = '100%';
        container.appendChild(canvas);

        // Create chart
        if (typeof Chart !== 'undefined') {
            new Chart(canvas, {
                type: 'pie',
                data: {
                    labels: labels,
                    datasets: [{
                        data: data,
                        backgroundColor: colors
                    }]
                }
            });
            console.log("Chart created successfully");
        } else {
            console.error("Chart.js not loaded");
        }
    }

    // Start trying
    tryCreateChart(1);
}

window.createSimpleChart = createSimpleChart;



