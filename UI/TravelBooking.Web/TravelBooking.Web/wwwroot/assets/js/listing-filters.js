/**
 * Dynamic filters for Hotel and Car listing pages.
 * Filters cards client-side by data attributes.
 */
(function (window, $) {
  "use strict";

  var hotelPriceMin = 0, hotelPriceMax = 3000;
  var carPriceMin = 0, carPriceMax = 3000;

  function applyHotelFilters() {
    var $cards = $("#hotel-listing-cards .hotel-card");
    if (!$cards.length) return;

    var priceMin = hotelPriceMin;
    var priceMax = hotelPriceMax;

    var starChecked = [];
    $("#hotel-filters input[id^='filter-star-']:checked").each(function () {
      starChecked.push(parseInt($(this).val(), 10));
    });

    var needWifi = $("#filter-wifi").is(":checked");
    var needParking = $("#filter-parking").is(":checked");
    var needPool = $("#filter-pool").is(":checked");
    var needRestaurant = $("#filter-restaurant").is(":checked");

    var minRating = 0;
    if ($("#filter-rating-9").is(":checked")) minRating = Math.max(minRating, 9);
    if ($("#filter-rating-8").is(":checked")) minRating = Math.max(minRating, 8);

    var visible = 0;
    $cards.each(function () {
      var $card = $(this);
      var price = parseFloat($card.attr("data-price")) || 0;
      var stars = parseInt($card.attr("data-stars"), 10) || 0;
      var rating = parseFloat($card.attr("data-rating")) || 0;
      var wifi = $card.attr("data-wifi") === "1";
      var parking = $card.attr("data-parking") === "1";
      var pool = $card.attr("data-pool") === "1";
      var restaurant = $card.attr("data-restaurant") === "1";

      if (price < priceMin || price > priceMax) {
        $card.hide();
        return;
      }
      if (starChecked.length > 0 && starChecked.indexOf(stars) === -1) {
        $card.hide();
        return;
      }
      if (needWifi && !wifi) {
        $card.hide();
        return;
      }
      if (needParking && !parking) {
        $card.hide();
        return;
      }
      if (needPool && !pool) {
        $card.hide();
        return;
      }
      if (needRestaurant && !restaurant) {
        $card.hide();
        return;
      }
      if (minRating > 0 && rating < minRating) {
        $card.hide();
        return;
      }
      $card.show();
      visible++;
    });

    var $noResults = $("#hotel-no-results");
    if ($noResults.length) {
      $noResults.toggle(visible === 0);
    }
  }

  function initHotelFilters() {
    if (!$("#hotel-listing-cards").length || !$("#hotel-price-slider").length) return;

    var $slider = $("#hotel-price-slider");
    if ($.fn.ionRangeSlider) {
      $slider.ionRangeSlider({
        type: "double",
        min: 0,
        max: 3000,
        from: 0,
        to: 3000,
        step: 50,
        hide_min_max: false,
        hide_from_to: false,
        prefix: "$",
        onFinish: function (data) {
          hotelPriceMin = data.from;
          hotelPriceMax = data.to;
          $("#hotel-price-min").text("$" + data.from);
          $("#hotel-price-max").text("$" + data.to);
          applyHotelFilters();
        },
        onChange: function (data) {
          hotelPriceMin = data.from;
          hotelPriceMax = data.to;
          $("#hotel-price-min").text("$" + data.from);
          $("#hotel-price-max").text("$" + data.to);
          applyHotelFilters();
        }
      });
      var inst = $slider.data("ionRangeSlider");
      if (inst) {
        hotelPriceMin = inst.result.from;
        hotelPriceMax = inst.result.to;
        $("#hotel-price-min").text("$" + hotelPriceMin);
        $("#hotel-price-max").text("$" + hotelPriceMax);
      }
    }

    $("#hotel-filters input[type='checkbox']").on("change", applyHotelFilters);
    applyHotelFilters();
  }

  function applyCarFilters() {
    var $cards = $("#car-listing-cards .car-card");
    if (!$cards.length) return;

    var priceMin = carPriceMin;
    var priceMax = carPriceMax;

    var categories = [];
    $("#car-filters input[name='filter-car-category']:checked").each(function () {
      categories.push($(this).val().toLowerCase());
    });

    var transmissions = [];
    $("#car-filters input[name='filter-transmission']:checked").each(function () {
      transmissions.push($(this).val().toLowerCase());
    });

    var fuelTypes = [];
    $("#car-filters input[name='filter-fuel']:checked").each(function () {
      fuelTypes.push($(this).val().toLowerCase());
    });

    var needAc = $("#filter-car-ac").is(":checked");
    var needGps = $("#filter-car-gps").is(":checked");

    var visible = 0;
    $cards.each(function () {
      var $card = $(this);
      var price = parseFloat($card.attr("data-price")) || 0;
      var category = (($card.attr("data-category")) || "").toLowerCase();
      var transmission = (($card.attr("data-transmission")) || "").toLowerCase();
      var fuel = (($card.attr("data-fuel")) || "").toLowerCase();
      var ac = $card.attr("data-ac") === "1";
      var gps = $card.attr("data-gps") === "1";

      if (price < priceMin || price > priceMax) {
        $card.hide();
        return;
      }
      if (categories.length > 0 && categories.indexOf(category) === -1) {
        $card.hide();
        return;
      }
      if (transmissions.length > 0 && transmissions.indexOf(transmission) === -1) {
        $card.hide();
        return;
      }
      if (fuelTypes.length > 0 && fuelTypes.indexOf(fuel) === -1) {
        $card.hide();
        return;
      }
      if (needAc && !ac) {
        $card.hide();
        return;
      }
      if (needGps && !gps) {
        $card.hide();
        return;
      }
      $card.show();
      visible++;
    });

    var $noResults = $("#car-no-results");
    if ($noResults.length) {
      $noResults.toggle(visible === 0);
    }
  }

  function initCarFilters() {
    if (!$("#car-listing-cards").length || !$("#car-price-slider").length) return;

    var $slider = $("#car-price-slider");
    if ($.fn.ionRangeSlider) {
      $slider.ionRangeSlider({
        type: "double",
        min: 0,
        max: 500,
        from: 0,
        to: 500,
        step: 10,
        hide_min_max: false,
        hide_from_to: false,
        prefix: "$",
        onFinish: function (data) {
          carPriceMin = data.from;
          carPriceMax = data.to;
          $("#car-price-min").text("$" + data.from);
          $("#car-price-max").text("$" + data.to);
          applyCarFilters();
        },
        onChange: function (data) {
          carPriceMin = data.from;
          carPriceMax = data.to;
          $("#car-price-min").text("$" + data.from);
          $("#car-price-max").text("$" + data.to);
          applyCarFilters();
        }
      });
      var inst = $slider.data("ionRangeSlider");
      if (inst) {
        carPriceMin = inst.result.from;
        carPriceMax = inst.result.to;
        $("#car-price-min").text("$" + carPriceMin);
        $("#car-price-max").text("$" + carPriceMax);
      }
    }

    $("#car-filters input[type='checkbox']").on("change", applyCarFilters);
    applyCarFilters();
  }

  window.initHotelFilters = initHotelFilters;
  window.initCarFilters = initCarFilters;
})(window, jQuery);
