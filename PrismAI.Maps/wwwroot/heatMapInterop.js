// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

export function showPrompt(message) {
  return prompt(message, 'Type anything here');
}
export function getLocation() {
    return new Promise((resolve, reject) => {
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(
                position => resolve(JSON.stringify(position.coords)),
                error => reject(error)
            );
        } else {
            reject(new Error('Geolocation is not supported by this browser.'));
        }
    });
}
export function initHeatmap(results) {
    if (!results) {
        console.error('Invalid heatmap data provided');
        return;
    }
    // Parse if string
    if (typeof results === 'string') {
        results = JSON.parse(results);
    }
    // Extract the array from the 'results' property if present
    if (results.results && Array.isArray(results.results)) {
        results = results.results;
    }
    console.log('Initializing heatmap with results:', results);
    // Convert JSON entries to WeightedLocation array
    const heatmapData = results.map(item => ({
        location: new google.maps.LatLng(
            item.location.latitude,
            item.location.longitude
        ),
        weight: item.query.popularity
    }));

    // Compute geographic bounds from data
    const bounds = new google.maps.LatLngBounds();
    heatmapData.forEach(point => bounds.extend(point.location));

    // Pad the bounds by a small margin
    const ne = bounds.getNorthEast();
    const sw = bounds.getSouthWest();
    const latPad = (ne.lat() - sw.lat()) * 0.1; // 10% padding
    const lngPad = (ne.lng() - sw.lng()) * 0.1; // 10% padding
    const paddedNE = new google.maps.LatLng(ne.lat() - latPad, ne.lng() - lngPad);
    const paddedSW = new google.maps.LatLng(sw.lat() + latPad, sw.lng() + lngPad);
    bounds.extend(paddedNE);
    bounds.extend(paddedSW);

    // Create the map centered on the computed center of bounds
    const map = new google.maps.Map(document.getElementById('map'), {
        center: bounds.getCenter(),
        zoom: 12,
        mapTypeId: 'roadmap'
    });

    // Adjust viewport to fit all points (with padding)
    map.fitBounds(bounds);

    // Render heatmap layer
    const heatmap = new google.maps.visualization.HeatmapLayer({
        data: heatmapData,
        map: map,
        radius: 30,
        opacity: 0.6
    });

    return { map, heatmap };
}

export async function initTaggedMap(model) {
    if (!model) {
        console.error('Invalid data');
        return;
    }
    // Parse if string
    if (typeof model === 'string') {
        model = JSON.parse(model);
    }
    console.log('Initializing tagged map with model:', model);
    // Extract anchors and place types
    const anchors = model.anchors || [];
    const placeTypes = model.places_query || [];
    if (!Array.isArray(anchors) || !Array.isArray(placeTypes) || anchors.length === 0 || placeTypes.length === 0) {
        console.error('No anchors or place types provided');
        return;
    }

    // Create the map and bounds
    const map = new google.maps.Map(document.getElementById('map'), {
        zoom: 13,
        center: { lat: anchors[0].lat, lng: anchors[0].lng }
    });
    const bounds = new google.maps.LatLngBounds();

    for (const anchor of anchors) {
        const anchorPos = { lat: anchor.lat, lng: anchor.lng };
        bounds.extend(anchorPos);
        for (const type of placeTypes) {
            // Prepare Nearby Search request using Place.searchNearby
            const request = {
                fields: ['displayName', 'location', 'rating','addressComponents', 'types'],
                locationRestriction: { center: anchorPos, radius: anchor.radius_m || anchor.radiusM || 100 },
                includedPrimaryTypes: [type],
                maxResultCount: 3,
                rankPreference: google.maps.places.SearchNearbyRankPreference.POPULARITY
            };
            try {
                const { places } = await google.maps.places.Place.searchNearby(request);
                for (const place of places) {
                    const marker = new google.maps.Marker({
                        map,
                        position: place.location,
                        title: place.displayName,
                    });
                    bounds.extend(place.location);
                    const infoContent = [];
                    infoContent.push(`<strong>${place.displayName}</strong>`);
                    infoContent.push(`Rating: ${place.rating ?? 'N/A'}`);
                    infoContent.push(
                        `Address: ${getAddressObject(place.addressComponents)}`
                    );
                    if (place.types && place.types.length) {
                        infoContent.push(`Type: ${place.types.join(', ')}`);
                    }
                    const info = new google.maps.InfoWindow({
                        content: infoContent.join('<br>')
                    });
                    marker.addListener('click', () => info.open(map, marker));
                }
            } catch (e) {
                console.error('searchNearby error', e);
            }
        }
    }
    map.fitBounds(bounds);
}

export function initRoutesMap(origin, destination) {
    const mapElId = document.getElementById('map');
    console.log('Map element ID:', mapElId); 
    const map = new google.maps.Map(mapElId, {
        zoom: 7,
        center: origin,             // Lat/Lng object literal is valid here
    });

    // ▶️ 4.  Request & render the route
    const service = new google.maps.DirectionsService();
    const renderer = new google.maps.DirectionsRenderer();

    service.route(
        {
            origin,
            destination,
            travelMode: google.maps.TravelMode.DRIVING, // or WALKING, etc.
        },
        (result, status) => {
            if (status === "OK") {
                renderer.setDirections(result);
            } else {
                console.error("Directions request failed: " + status);
            }
        }
    );
    console.log('Map:', map);
    renderer.setMap(map);
    renderer.setPanel(document.getElementById('directionsPanel')); // Optional: to show route steps in a panel
}
function getAddressObject(address_components) {
    // Extract address parts from address_components array
    if (!Array.isArray(address_components)) return '';
    let streetNumber = '', route = '', city = '', state = '', postalCode = '';
    for (const comp of address_components) {
        if (comp.types && Array.isArray(comp.types)) {
            if (comp.types.includes('street_number')) {
                streetNumber = comp.shortText || comp.longText || '';
            } else if (comp.types.includes('route')) {
                route = comp.shortText || comp.longText || '';
            } else if (comp.types.includes('locality')) {
                city = comp.shortText || comp.longText || '';
            } else if (comp.types.includes('administrative_area_level_1')) {
                state = comp.shortText || comp.longText || '';
            } else if (comp.types.includes('postal_code')) {
                postalCode = comp.shortText || comp.longText || '';
            }
        }
    }
    // Format: 310 Riverside Dr, New York, NY 10025
    let address = '';
    if (streetNumber || route) {
        address += `${streetNumber} ${route}`.trim();
    }
    if (city) {
        address += address ? `, ${city}` : city;
    }
    if (state) {
        address += address ? `, ${state}` : state;
    }
    if (postalCode) {
        address += address ? ` ${postalCode}` : postalCode;
    }
    return address.trim();
}