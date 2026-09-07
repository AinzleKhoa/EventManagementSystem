let currentStep = 1;
var defaultLat = window.defaultLat || 10.7769;
var defaultLng = window.defaultLng || 106.7009;
var map, marker;
let ticketIdx = window.ticketIdx || 0;

// Set từ Razor nếu có
if (document.querySelector('[data-lat]')) {
    defaultLat = parseFloat(document.querySelector('[data-lat]').dataset.lat);
    defaultLng = parseFloat(document.querySelector('[data-lng]').dataset.lng);
    ticketIdx = parseInt(document.querySelector('[data-ticketidx]').dataset.ticketidx);
}

document.addEventListener('DOMContentLoaded', function() {
    initLeafletMap();
    initAddressSearch();
    updateLivePreview();
    initFormValidation();
});

function goToStep(targetStep) {
    if (targetStep > currentStep) {
        if (currentStep <= 1 && targetStep > 1) {
            if (!validateStep1()) return;
        }
        if (currentStep <= 2 && targetStep > 2) {
            if (!validateStep2()) return;
        }
    }
    hideErrorAlert();
    currentStep = targetStep;
    document.querySelectorAll('.step-pane').forEach(el => el.style.display = 'none');
    document.getElementById('stepPane' + targetStep).style.display = 'block';
    document.querySelectorAll('.wizard-step').forEach((el, index) => {
        const stepNum = index + 1;
        el.classList.remove('active', 'completed');
        if (stepNum === targetStep) el.classList.add('active');
        else if (stepNum < targetStep) el.classList.add('completed');
    });
    if (targetStep === 2 && map) {
        setTimeout(() => map.invalidateSize(), 250);
    }
}

function hideErrorAlert() {
    const errDiv = document.getElementById('stepErrorAlert');
    if (errDiv) errDiv.style.display = 'none';
}

function showErrorAlert(msg) {
    const errDiv = document.getElementById('stepErrorAlert');
    const errText = document.getElementById('stepErrorText');
    if (errDiv && errText) {
        errText.innerText = msg;
        errDiv.style.display = 'block';
        errDiv.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
}

function validateStep1() {
    hideErrorAlert();
    const titleInput = document.getElementById('inputTitle');
    if (!titleInput || !titleInput.value.trim() || titleInput.value.trim().length < 3) {
        titleInput.classList.add('is-invalid');
        showErrorAlert('Event Title is required and must be at least 3 characters long.');
        titleInput.focus();
        return false;
    }
    titleInput.classList.remove('is-invalid');
    return true;
}

function validateStep2() {
    hideErrorAlert();
    const startInput = document.getElementById('inputStartDate');
    const endInput = document.getElementById('inputEndDate');
    const locationInput = document.getElementById('locationInput');

    if (!startInput || !startInput.value) {
        startInput.classList.add('is-invalid');
        showErrorAlert('Event Start Date & Time is required.');
        startInput.focus();
        return false;
    }
    
    const now = new Date();
    if (new Date(startInput.value) <= now) {
        startInput.classList.add('is-invalid');
        showErrorAlert('Event Start Date & Time must be in the future.');
        startInput.focus();
        return false;
    }
    startInput.classList.remove('is-invalid');

    if (!endInput || !endInput.value) {
        endInput.classList.add('is-invalid');
        showErrorAlert('Event End Date & Time is required.');
        endInput.focus();
        return false;
    }

    if (new Date(endInput.value) <= new Date(startInput.value)) {
        endInput.classList.add('is-invalid');
        showErrorAlert('Event End Date & Time must be after Start Date & Time.');
        endInput.focus();
        return false;
    }
    endInput.classList.remove('is-invalid');

    if (!locationInput || !locationInput.value.trim()) {
        locationInput.classList.add('is-invalid');
        showErrorAlert('Venue Location address is required.');
        locationInput.focus();
        return false;
    }
    locationInput.classList.remove('is-invalid');
    return true;
}

function validateStep3() {
    hideErrorAlert();
    const ticketRows = document.querySelectorAll('#ticketList tr');
    if (ticketRows.length === 0) {
        showErrorAlert('At least one Ticket Type is required for the event.');
        return false;
    }

    const endDateVal = document.getElementById('inputEndDate').value;
    const eventEnd = endDateVal ? new Date(endDateVal) : null;

    for (let i = 0; i < ticketRows.length; i++) {
        const row = ticketRows[i];
        const nameInp = row.querySelector('input[name*="TypeName"]');
        const priceInp = row.querySelector('input[name*="Price"]');
        const qtyInp = row.querySelector('input[name*="Quantity"]');
        const saleStartInp = row.querySelector('input[name*="SaleStartDate"]');
        const saleEndInp = row.querySelector('input[name*="SaleEndDate"]');

        if (nameInp && !nameInp.value.trim()) {
            nameInp.classList.add('is-invalid');
            showErrorAlert(`Ticket #${i + 1}: Ticket Name is required.`);
            nameInp.focus();
            return false;
        }

        if (priceInp && (isNaN(priceInp.value) || Number(priceInp.value) < 0)) {
            priceInp.classList.add('is-invalid');
            showErrorAlert(`Ticket #${i + 1}: Price must be >= 0.`);
            priceInp.focus();
            return false;
        }

        if (qtyInp && (isNaN(qtyInp.value) || Number(qtyInp.value) < 1)) {
            qtyInp.classList.add('is-invalid');
            showErrorAlert(`Ticket #${i + 1}: Quantity must be at least 1.`);
            qtyInp.focus();
            return false;
        }

        const saleStart = saleStartInp && saleStartInp.value ? new Date(saleStartInp.value) : null;
        const saleEnd = saleEndInp && saleEndInp.value ? new Date(saleEndInp.value) : null;

        if (saleStart && eventEnd && saleStart >= eventEnd) {
            saleStartInp.classList.add('is-invalid');
            showErrorAlert(`Ticket #${i + 1}: Sale Start Date must be before Event End Date.`);
            saleStartInp.focus();
            return false;
        }

        if (saleStart && saleEnd && saleEnd <= saleStart) {
            saleEndInp.classList.add('is-invalid');
            showErrorAlert(`Ticket #${i + 1}: Sale End Date must be after Sale Start Date.`);
            saleEndInp.focus();
            return false;
        }

        if (eventEnd && saleEnd && saleEnd > eventEnd) {
            saleEndInp.classList.add('is-invalid');
            showErrorAlert(`Ticket #${i + 1}: Sale End Date cannot be after Event End Date.`);
            saleEndInp.focus();
            return false;
        }
    }
    return true;
}

function updateLivePreview() {
    const title = document.getElementById('inputTitle').value || 'Event Title Placeholder';
    document.getElementById('livePreviewTitle').innerText = title;

    const location = document.getElementById('locationInput').value || 'Venue location will appear here...';
    document.getElementById('livePreviewLocation').innerHTML = `<i class="fa-solid fa-location-dot text-danger me-1"></i> ${location}`;

    const startDateVal = document.getElementById('inputStartDate').value;
    if (startDateVal) {
        const d = new Date(startDateVal);
        document.getElementById('livePreviewDate').innerHTML = `<i class="fa-regular fa-calendar me-1"></i> ${d.toLocaleDateString('en-US', { weekday: 'short', month: 'short', day: 'numeric' })} • ${d.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })}`;
    }

    const priceInputs = document.querySelectorAll('#ticketList input[name*=".Price"]');
    let minPrice = Infinity;
    priceInputs.forEach(inp => {
        const val = parseFloat(inp.value);
        if (!isNaN(val) && val < minPrice) minPrice = val;
    });

    if (minPrice === Infinity) {
        document.getElementById('livePreviewPrice').innerText = 'Contact';
    } else if (minPrice === 0) {
        document.getElementById('livePreviewPrice').innerText = 'FREE';
    } else {
        document.getElementById('livePreviewPrice').innerText = minPrice.toLocaleString('vi-VN') + ' ₫';
    }
}

function previewUploadImage(input) {
    if (input.files && input.files[0]) {
        const url = URL.createObjectURL(input.files[0]);
        document.getElementById('livePreviewImg').src = url;
        document.getElementById('inputPosterUrl').value = '';
    }
}

function selectStockCover(url) {
    document.getElementById('livePreviewImg').src = url;
    document.getElementById('inputPosterUrl').value = url;
    bootstrap.Modal.getInstance(document.getElementById('unsplashModal')).hide();
}

function generateAiDescription() {
    const title = document.getElementById('inputTitle').value || 'Upcoming Event';
    const generated = `🎉 Welcome to ${title}!\n\nJoin us for an unmissable experience bringing together enthusiasts, experts, and great vibes.\n\n✨ Event Highlights:\n• Live performances & interactive sessions\n• Networking opportunities with industry leaders\n• Exclusive merchandise and gifts for attendees\n\n📌 Agenda & Important Info:\n- Please present your digital QR ticket at the check-in desk.\n- Doors open 30 minutes prior to the official start time.\n\nGet your tickets now before they sell out!`;
    document.getElementById('inputDescription').value = generated;
    updateLivePreview();
}

function addPresetTicket(type) {
    const startVal = document.getElementById('inputStartDate').value;
    const endVal = document.getElementById('inputEndDate').value;

    if (type === 'free') {
        createTicketRowObj("General Admission (Free)", 0, 100, "#10b981", startVal, endVal);
    } else if (type === 'early') {
        createTicketRowObj("Early Bird Discount", 100000, 50, "#f59e0b", startVal, endVal);
    } else if (type === 'vip') {
        createTicketRowObj("VIP FanZone Pass", 500000, 30, "#8b5cf6", startVal, endVal);
    }
}

function createTicketRowObj(name, price, qty, color, start, end) {
    const row = document.createElement('tr');
    row.innerHTML = `
    <td><input name="TicketTypes[${ticketIdx}].TypeName" value="${name}" class="form-control form-control-sm" required maxlength="100" oninput="updateLivePreview()" /></td>
    <td><input name="TicketTypes[${ticketIdx}].Price" type="number" value="${price}" class="form-control form-control-sm" min="0" max="100000000" oninput="updateLivePreview()" /></td>
    <td><input name="TicketTypes[${ticketIdx}].Quantity" type="number" value="${qty}" class="form-control form-control-sm" min="1" max="100000" /></td>
    <td><input name="TicketTypes[${ticketIdx}].ColorCode" type="color" value="${color}" class="form-control form-control-color w-100" /></td>
    <td><input name="TicketTypes[${ticketIdx}].SaleStartDate" type="datetime-local" value="${start}" class="form-control form-control-sm" /></td>
    <td><input name="TicketTypes[${ticketIdx}].SaleEndDate" type="datetime-local" value="${end}" class="form-control form-control-sm" /></td>
    <td><button type="button" class="btn text-danger btn-sm" onclick="this.closest('tr').remove(); updateLivePreview();"><i class="fa fa-trash"></i></button></td>
    `;
    document.getElementById('ticketList').appendChild(row);
    ticketIdx++;
    updateLivePreview();
}

function initLeafletMap() {
    map = L.map('map').setView([defaultLat, defaultLng], 15);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '© OpenStreetMap'
    }).addTo(map);

    marker = L.marker([defaultLat, defaultLng], { draggable: true }).addTo(map);
    marker.on('dragend', function() {
        var pos = marker.getLatLng();
        updateCoordinates(pos.lat, pos.lng);
    });
    map.on('click', function(e) {
        marker.setLatLng(e.latlng);
        updateCoordinates(e.latlng.lat, e.latlng.lng);
    });
}

function initAddressSearch() {
    const searchBox = document.getElementById('searchBox');
    const searchResults = document.getElementById('searchResults');
    if (searchBox && searchResults) {
        let timer;
        searchBox.addEventListener('input', function() {
            clearTimeout(timer);
            const q = this.value.trim();
            if (q.length < 3) {
                searchResults.style.display = 'none';
                return;
            }
            timer = setTimeout(function() {
                fetch('https://nominatim.openstreetmap.org/search?format=json&countrycodes=vn&q=' + encodeURIComponent(q))
                    .then(res => res.json())
                    .then(data => {
                        searchResults.innerHTML = '';
                        if (data && data.length > 0) {
                            searchResults.style.display = 'block';
                            data.slice(0, 5).forEach(item => {
                                const div = document.createElement('div');
                                div.innerText = item.display_name;
                                div.onclick = function() {
                                    const itemLat = parseFloat(item.lat);
                                    const itemLon = parseFloat(item.lon);
                                    map.flyTo([itemLat, itemLon], 16);
                                    marker.setLatLng([itemLat, itemLon]);
                                    updateCoordinates(itemLat, itemLon);
                                    searchResults.style.display = 'none';
                                };
                                searchResults.appendChild(div);
                            });
                        }
                    }).catch(() => { searchResults.style.display = 'none'; });
            }, 400);
        });
    }
}

function updateCoordinates(lat, lng) {
    document.getElementById('latitudeInput').value = lat.toFixed(8);
    document.getElementById('longitudeInput').value = lng.toFixed(8);
    fetch('https://nominatim.openstreetmap.org/reverse?format=json&lat=' + lat + '&lon=' + lng)
        .then(res => res.json())
        .then(data => {
            if (data && data.display_name) {
                document.getElementById('locationInput').value = data.display_name;
                updateLivePreview();
            }
        }).catch(err => console.error(err));
}

function deleteTicket(id) {
    if (confirm('Are you sure you want to delete this ticket type?')) {
        fetch('/Events/AjaxDeleteTicket?id=' + id, { method: 'POST' })
            .then(res => res.json())
            .then(data => {
                if (data.success) location.reload();
                else alert('Could not delete ticket type: ' + data.message);
            });
    }
}

function initFormValidation() {
    const form = document.getElementById('eventUpdateForm');
    if (!form) return;
    form.addEventListener('submit', function(e) {
        if (!validateStep1()) {
            e.preventDefault();
            goToStep(1);
            return;
        }
        if (!validateStep2()) {
            e.preventDefault();
            goToStep(2);
            return;
        }
        if (!validateStep3()) {
            e.preventDefault();
            goToStep(3);
            return;
        }
    });
}